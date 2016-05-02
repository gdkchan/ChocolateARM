using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ChocolateARM.RPi
{
    /// <summary>
    ///     Represents the VideoCore IV GPU.
    ///     More specifically, it just handles basic Frame Buffer data, so it doesn't actually emualtes de GPU.
    /// </summary>
    public class VideoCore : IO32
    {
        private const uint ARM_MemoryBase = 0;
        private const uint ARM_MemoryLength = 0x8000000;
        private const uint VC_MemoryBase = 0x8000000;
        private const uint VC_MemoryLength = 0x8000000;
        private const uint VC_FrameBufferOffset = 0x100000;

        private Memory Parent;
        private Queue<uint> Mailbox;

        /// <summary>
        ///     Creates a new instace of the RaspberryPi VC (VideoCore).
        /// </summary>
        /// <param name="Parent">The parent RPi Memory manager</param>
        public VideoCore(Memory Parent)
        {
            this.Parent = Parent;
            Mailbox = new Queue<uint>();

            FramePalette = new Color[256];
        }

        /// <summary>
        ///     Reads a 32-bits value from the Video Core.
        /// </summary>
        /// <param name="Address">The Address being read</param>
        /// <returns>The value at the given Address</returns>
        public override uint ProcessRead(uint Address)
        {
            uint Value = 0;

            switch (Address)
            {
                case 0x2000b880: if (Mailbox.Count > 0) Value = Mailbox.Dequeue(); break;
                case 0x2000b890: if (Mailbox.Count > 0) Value = Mailbox.Peek(); break;
                case 0x2000b898:
                    if (Mailbox.Count == 0) Value = 0x40000000;
                    if (Mailbox.Count == 8) Value |= 0x80000000;
                    break;
            }

            return Value;
        }

        public uint ScreenPhysicalWidth = 640;
        public uint ScreenPhysicalHeight = 480;
        public uint ScreenVirtualWidth = 640;
        public uint ScreenVirtualHeight = 480;
        public uint ScreenStride = 2560;
        public uint ScreenBitsPerPixel = 32;
        public uint ScreenOffsetX;
        public uint ScreenOffsetY;
        public uint FrameBufferAddress;
        public uint FrameBufferLength;
        public Color[] FramePalette;

        /// <summary>
        ///     Occurs when the Frame Buffer configuration is changed.
        /// </summary>
        public event Action OnScreenSetup;

        private enum Tag : uint
        {
            End = 0,
            GetBoardModel = 0x10001,
            GetBoardRevision = 0x10002,
            GetBoardMACAddress = 0x10003,
            GetBoardSerial = 0x10004,
            GetARMMemory = 0x10005,
            GetVCMemory = 0x10006,
            AllocateBuffer = 0x40001,
            BlankScreen = 0x40002,
            GetPhysicalDisplay = 0x40003,
            GetVirtualBuffer = 0x40004,
            GetBitsPerPixel = 0x40005,
            GetStride = 0x40008,
            GetVirtualOffset = 0x40009,
            GetPalette = 0x4000b,
            GetTouchBuffer = 0x4000f,
            TestPhysicalDisplay = 0x44003,
            TestVirtualBuffer = 0x44004,
            TestBitsPerPixel = 0x44005,
            TestVirtualOffset = 0x44009,
            TestPalette = 0x4400b,
            SetPhysicalDisplay = 0x48003,
            SetVirtualBuffer = 0x48004,
            SetBitsPerPixel = 0x48005,
            SetVirtualOffset = 0x48009,
            SetPalette = 0x4800b,
            GetDMAChannels = 0x60001
        }

        /*
         * Frame Buffer setup and other Spare Parts.
         */

        /// <summary>
        ///     Writes a 32-bits value to a given Address.
        /// </summary>
        /// <param name="Address">The Address being written</param>
        /// <param name="Value">The value being written</param>
        public override void ProcessWrite(uint Address, uint Value)
        {
            byte Channel = (byte)(Value & 0xf);
            uint BufferAddress = Value & 0xfffffff0;

            System.Diagnostics.Debug.WriteLine("fb setup! " + Channel.ToString());

            switch (Channel)
            {
                case 0: //Power Management
                    //No idea of what this is supposed to do, so just return a OK
                    if (Mailbox.Count < 8) Mailbox.Enqueue(Channel);
                    break;

                case 1: //Frame Buffer
                    //Read the values that the program gave to us
                    ScreenPhysicalWidth = Parent.ReadUInt32(BufferAddress);
                    ScreenPhysicalHeight = Parent.ReadUInt32(BufferAddress + 4);
                    ScreenVirtualWidth = Parent.ReadUInt32(BufferAddress + 8);
                    ScreenVirtualHeight = Parent.ReadUInt32(BufferAddress + 0xc);
                    ScreenStride = Parent.ReadUInt32(BufferAddress + 0x10);
                    ScreenBitsPerPixel = Parent.ReadUInt32(BufferAddress + 0x14);
                    ScreenOffsetX = Parent.ReadUInt32(BufferAddress + 0x18);
                    ScreenOffsetY = Parent.ReadUInt32(BufferAddress + 0x1c);
                    FrameBufferAddress = Parent.ReadUInt32(BufferAddress + 0x20);
                    FrameBufferLength = Parent.ReadUInt32(BufferAddress + 0x24);
                    if (OnScreenSetup != null) OnScreenSetup();

                    //Now write back the values that should be given to the program
                    ScreenStride = ScreenVirtualWidth * (ScreenBitsPerPixel >> 3);
                    FrameBufferAddress = (VC_MemoryBase + VC_FrameBufferOffset) | 0xc0000000;
                    FrameBufferLength = ScreenVirtualHeight * ScreenStride;

                    Parent.WriteUInt32(BufferAddress + 0x10, ScreenStride);
                    Parent.WriteUInt32(BufferAddress + 0x20, FrameBufferAddress);
                    Parent.WriteUInt32(BufferAddress + 0x24, FrameBufferLength);

                    if (Mailbox.Count < 8) Mailbox.Enqueue(Channel);
                    break;

                case 8: ParseBuffer(BufferAddress, Channel); break; //ARM to VC request
            }
        }

        /// <summary>
        ///     Parses a list of concatenated Request Buffers.
        /// </summary>
        /// <param name="Address">The Address where the buffers are located</param>
        /// <param name="Channel">The Channel being written</param>
        private void ParseBuffer(uint Address, byte Channel)
        {
            uint StartAddress = Address;
            uint BufferLength = Parent.ReadUInt32(Address);
            bool IsRequest = (Parent.ReadUInt32(Address + 4) & 0x80000000) != 0;
            Address += 8;

            while (Address < StartAddress + BufferLength)
            {
                Tag Tag = (Tag)Parent.ReadUInt32(Address);
                if (Tag == Tag.End) break;

                System.Diagnostics.Debug.WriteLine(Parent.ReadUInt32(Address).ToString("X8"));

                //Read the data Buffer
                uint Length = Parent.ReadUInt32(Address + 4);
                uint[] Buffer = new uint[Length >> 2];
                int BufferEntry = 0;
                for (uint Offset = 0; Offset < Length ; Offset += 4)
                {
                    Buffer[BufferEntry++] = Parent.ReadUInt32(Address + 0xc + Offset);
                }

                //If this is a Request Header, then parses the Data according to the Tag
                if ((Parent.ReadUInt32(Address + 8) & 0x80000000) == 0)
                {
                    switch (Tag)
                    {
                        case Tag.GetBoardModel: CreateBuffer(Address, Tag, 0); break;

                        case Tag.GetBoardRevision: CreateBuffer(Address, Tag, 2); break;

                        case Tag.GetBoardMACAddress: CreateBuffer8(Address, Tag, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff); break;

                        case Tag.GetBoardSerial: CreateBuffer(Address, Tag, 0x00112233, 0xaabbccdd); break;

                        case Tag.GetARMMemory: CreateBuffer(Address, Tag, ARM_MemoryBase, ARM_MemoryLength); break;

                        case Tag.GetVCMemory: CreateBuffer(Address, Tag, VC_MemoryBase, VC_MemoryLength); break;

                        case Tag.AllocateBuffer:
                            uint Align = Parent.ReadUInt32(Address + 0xc);
                            FrameBufferAddress = (VC_MemoryBase + VC_FrameBufferOffset) | 0xc0000000;
                            if (Align > 0) FrameBufferAddress &= ~(Align - 1);
                            ScreenStride = ScreenVirtualWidth * (ScreenBitsPerPixel >> 3);
                            FrameBufferLength = ScreenVirtualHeight * ScreenStride;
                            CreateBuffer(Address, Tag, FrameBufferAddress, FrameBufferLength);
                            break;

                        case Tag.BlankScreen:
                            bool State = (Buffer[0] & 1) != 0;
                            if (State)
                            {
                                for (uint Offset = 0; Offset < FrameBufferLength; Offset++)
                                {
                                    Parent.WriteUInt8(FrameBufferAddress + Offset, 0);
                                }
                                CreateBuffer(Address, Tag, 1);
                            }
                            else
                                CreateBuffer(Address, Tag, 0);
                            break;

                        case Tag.GetPhysicalDisplay: CreateBuffer(Address, Tag, ScreenPhysicalWidth, ScreenPhysicalHeight); break;

                        case Tag.GetVirtualBuffer: CreateBuffer(Address, Tag, ScreenVirtualWidth, ScreenVirtualHeight); break;

                        case Tag.GetBitsPerPixel: CreateBuffer(Address, Tag, ScreenBitsPerPixel); break;

                        case Tag.GetStride: CreateBuffer(Address, Tag, ScreenStride); break;

                        case Tag.GetVirtualOffset: CreateBuffer(Address, Tag, ScreenOffsetX, ScreenOffsetY); break;

                        case Tag.GetPalette:
                            uint[] PaletteBuffer = new uint[1024];
                            for (int Index = 0; Index < 1024; Index++)
                            {
                                uint Color = FramePalette[Index].A;
                                Color |= (uint)(FramePalette[Index].B << 8);
                                Color |= (uint)(FramePalette[Index].G << 16);
                                Color |= (uint)(FramePalette[Index].R << 24);

                                PaletteBuffer[Index] = Color;
                            }
                            CreateBuffer(Address, Tag, PaletteBuffer);
                            break;

                        case Tag.GetTouchBuffer: CreateBuffer(Address, Tag, 0, 0); break;

                        case Tag.TestPhysicalDisplay: CreateBuffer(Address, Tag, Buffer); break;

                        case Tag.TestVirtualBuffer: CreateBuffer(Address, Tag, Buffer); break;

                        case Tag.TestBitsPerPixel: CreateBuffer(Address, Tag, Buffer); break;

                        case Tag.TestVirtualOffset: CreateBuffer(Address, Tag, Buffer); break;

                        case Tag.TestPalette: CreateBuffer(Address, Tag, 0); break;

                        case Tag.SetPhysicalDisplay:
                            ScreenPhysicalWidth = Buffer[0];
                            ScreenPhysicalHeight = Buffer[1];
                            CreateBuffer(Address, Tag, Buffer);
                            break;

                        case Tag.SetVirtualBuffer:
                            ScreenVirtualWidth = Buffer[0];
                            ScreenVirtualHeight = Buffer[1];
                            CreateBuffer(Address, Tag, Buffer);
                            if (OnScreenSetup != null) OnScreenSetup();
                            break;

                        case Tag.SetBitsPerPixel:
                            ScreenBitsPerPixel = Buffer[0];
                            CreateBuffer(Address, Tag, Buffer);
                            break;

                        case Tag.SetVirtualOffset:
                            ScreenOffsetX = Buffer[0];
                            ScreenOffsetY = Buffer[1];
                            CreateBuffer(Address, Tag, Buffer);
                            break;

                        case Tag.SetPalette:
                            uint StartIndex = Buffer[0];
                            uint Entries = Buffer[1];
                            for (int Index = 0; Index < Entries; Index++)
                            {
                                byte R = (byte)(Buffer[2 + Index] >> 24);
                                byte G = (byte)(Buffer[2 + Index] >> 16);
                                byte B = (byte)(Buffer[2 + Index] >> 8);
                                byte A = (byte)Buffer[2 + Index];

                                FramePalette[StartIndex + Index] = Color.FromArgb(A, R, G, B);
                            }
                            CreateBuffer(Address, Tag, 0);
                            break;

                        case Tag.GetDMAChannels: CreateBuffer(Address, Tag, 0xffff); break;
                    }
                }

                Address += Length + 0xc;
            }

            Parent.WriteUInt32(StartAddress + 4, 0x80000000); //Set response code
            if (Mailbox.Count < 8) Mailbox.Enqueue(Channel); //No error (0)
        }

        /// <summary>
        ///     Creates a Response Buffer on the Memory.
        /// </summary>
        /// <param name="Address">The Address to write the Response buffer</param>
        /// <param name="Tag">The Tag of the Response buffer</param>
        /// <param name="Values">The Values contained on the Response buffer</param>
        private void CreateBuffer(uint Address, Tag Tag, params uint[] Values)
        {
            int Length = Values.Length << 2;
            Parent.WriteUInt32(Address, (uint)Tag);
            Parent.WriteUInt32(Address + 4, (uint)Length);
            Parent.WriteUInt32(Address + 8, (uint)(Length & 0x7fffffff) | 0x80000000);
            Address += 0xc;

            foreach (uint Value in Values)
            {
                Parent.WriteUInt32(Address, Value);
                Address += 4;
            }
        }

        /// <summary>
        ///     Creates a Response Buffer on the Memory.
        /// </summary>
        /// <param name="Address">The Address to write the Response buffer</param>
        /// <param name="Tag">The Tag of the Response buffer</param>
        /// <param name="Values">The Values contained on the Response buffer</param>
        private void CreateBuffer8(uint Address, Tag Tag, params byte[] Values)
        {
            Parent.WriteUInt32(Address, (uint)Tag);
            Parent.WriteUInt32(Address + 4, (uint)Values.Length);
            Parent.WriteUInt32(Address + 8, (uint)(Values.Length & 0x7fffffff) | 0x80000000);
            Address += 0xc;

            foreach (byte Value in Values)
            {
                Parent.WriteUInt8(Address, Value);
                Address++;
            }
        }

        /*
         * Frame Buffer Image rendering.
         */

        /// <summary>
        ///     Gets the current Screen frame from the Frame Buffer.
        ///     It will return null if the Frame Buffer hasn't been initialized yet.
        /// </summary>
        public Bitmap GetImage()
        {
            if (FrameBufferLength > 0)
            {
                uint Address = FrameBufferAddress;
                uint Length = FrameBufferLength;
                int Width = (int)ScreenVirtualWidth;
                int Height = (int)ScreenVirtualHeight;

                byte[] FrameBuffer = Parent.GetData(Address, Length);
                byte[] Buffer = new byte[Width * Height * 4];

                uint InputOffset = 0;
                uint OutputOffset = 0;
                switch (ScreenBitsPerPixel)
                {
                    case 8:
                        for (int Y = 0; Y < Height; Y++)
                        {
                            for (int X = 0; X < Width; X++)
                            {
                                byte Index = FrameBuffer[InputOffset++];
                                Color PaletteColor = FramePalette[Index];

                                Buffer[OutputOffset] = PaletteColor.B;
                                Buffer[OutputOffset + 1] = PaletteColor.G;
                                Buffer[OutputOffset + 2] = PaletteColor.R;
                                Buffer[OutputOffset + 3] = PaletteColor.A;

                                OutputOffset += 4;
                            }
                        }
                        break;

                    case 15:
                        for (int Y = 0; Y < Height; Y++)
                        {
                            for (int X = 0; X < Width; X++)
                            {
                                ushort RGB = (ushort)(FrameBuffer[InputOffset] | (FrameBuffer[InputOffset + 1] << 8));

                                byte R = (byte)(((RGB >> 1) & 0x1f) << 3);
                                byte G = (byte)(((RGB >> 6) & 0x1f) << 3);
                                byte B = (byte)(((RGB >> 11) & 0x1f) << 3);

                                Buffer[OutputOffset] = (byte)(R | (R >> 5));
                                Buffer[OutputOffset + 1] = (byte)(G | (G >> 5));
                                Buffer[OutputOffset + 2] = (byte)(B | (B >> 5));
                                Buffer[OutputOffset + 3] = 0xff;

                                InputOffset += 2;
                                OutputOffset += 4;
                            }
                        }
                        break;

                    case 16:
                        for (int Y = 0; Y < Height; Y++)
                        {
                            for (int X = 0; X < Width; X++)
                            {
                                ushort RGB = (ushort)(FrameBuffer[InputOffset] | (FrameBuffer[InputOffset + 1] << 8));

                                byte R = (byte)((RGB & 0x1f) << 3);
                                byte G = (byte)(((RGB >> 5) & 0x3f) << 2);
                                byte B = (byte)(((RGB >> 11) & 0x1f) << 3);

                                Buffer[OutputOffset] = (byte)(R | (R >> 5));
                                Buffer[OutputOffset + 1] = (byte)(G | (G >> 6));
                                Buffer[OutputOffset + 2] = (byte)(B | (B >> 5));
                                Buffer[OutputOffset + 3] = 0xff;

                                InputOffset += 2;
                                OutputOffset += 4;
                            }
                        }
                        break;

                    case 24:
                        for (int Y = 0; Y < Height; Y++)
                        {
                            for (int X = 0; X < Width; X++)
                            {
                                Buffer[OutputOffset] = FrameBuffer[InputOffset + 2];
                                Buffer[OutputOffset + 1] = FrameBuffer[InputOffset + 1];
                                Buffer[OutputOffset + 2] = FrameBuffer[InputOffset];
                                Buffer[OutputOffset + 3] = 0xff;

                                InputOffset += 3;
                                OutputOffset += 4;
                            }
                        }
                        break;

                    case 32:
                        for (int Y = 0; Y < Height; Y++)
                        {
                            for (int X = 0; X < Width; X++)
                            {
                                Buffer[InputOffset] = FrameBuffer[InputOffset + 2];
                                Buffer[InputOffset + 1] = FrameBuffer[InputOffset + 1];
                                Buffer[InputOffset + 2] = FrameBuffer[InputOffset];
                                Buffer[InputOffset + 3] = FrameBuffer[InputOffset + 3];

                                InputOffset += 4;
                            }
                        }
                        break;
                }

                Bitmap Img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                Rectangle ImgRect = new Rectangle(0, 0, Img.Width, Img.Height);
                BitmapData ImgData = Img.LockBits(ImgRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(Buffer, 0, ImgData.Scan0, Buffer.Length);
                Img.UnlockBits(ImgData);
                return Img;
            }

            return null;
        }
    }
}
