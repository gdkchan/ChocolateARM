using System;

using SharpARM.ARM11;

using ChocolateARM.RPi.Peripherals;

namespace ChocolateARM.RPi
{
    /// <summary>
    ///     Represents the Memory and IO of the BCM2835 SoC.
    /// </summary>
    public class Memory : IBus
    {
        const uint KB = 1024;
        const uint MB = 1024 * KB;

        public byte[] SDRAM;
        
        public RPiDMA DMA;
        public RPiTimer Timer;
        public VideoCore Video;
        public RPiUART UART;
        public RPiMiniUART MiniUART;

        /// <summary>
        ///     Creates a new instance of the RaspberryPi memory manager.
        /// </summary>
        public Memory()
        {
            SDRAM = new byte[256 * MB];
            
            DMA = new RPiDMA(this);
            Timer = new RPiTimer();
            Video = new VideoCore(this);
            UART = new RPiUART();
            MiniUART = new RPiMiniUART(this);
        }

        /// <summary>
        ///     Reads 8-bits from Memory.
        /// </summary>
        /// <param name="Address">The Address to read</param>
        /// <returns>The value at the given Address</returns>
        public byte ReadUInt8(uint Address)
        {
            Address &= 0x3fffffff;

            if (Address < SDRAM.Length)
                return SDRAM[Address];
            else if (Address >= 0x20007000 && Address < 0x20008000)
                return DMA.Read(Address);
            else if (Address >= 0x2000b400 && Address < 0x2000b424)
                return Timer.Read(Address);
            else if (Address >= 0x2000b880 && Address < 0x2000b8b0)
                return Video.Read(Address);
            else if (Address >= 0x20201000 && Address < 0x20201090)
                return UART.Read(Address);
            else if (Address >= 0x20215000 && Address < 0x20215080)
                return MiniUART.Read(Address);

            return 0;
        }

        /// <summary>
        ///     Reads 32-bits from Memory.
        /// </summary>
        /// <param name="Address">The Address to read</param>
        /// <returns>The value at the given Address</returns>
        public uint ReadUInt32(uint Address)
        {
            return (uint)(ReadUInt8(Address) |
                (ReadUInt8(Address + 1) << 8) |
                (ReadUInt8(Address + 2) << 16) |
                (ReadUInt8(Address + 3) << 24));
        }

        /// <summary>
        ///     Writes 8-bits to Memory.
        /// </summary>
        /// <param name="Address">The Address to write the value into</param>
        /// <param name="Value">The value to be written</param>
        public void WriteUInt8(uint Address, byte Value)
        {
            Address &= 0x3fffffff;

            if (Address < SDRAM.Length)
                SDRAM[Address] = Value;
            else if (Address >= 0x20007000 && Address < 0x20008000)
                DMA.Write(Address, Value);
            else if (Address >= 0x2000b400 && Address < 0x2000b424)
                Timer.Write(Address, Value);
            else if (Address >= 0x2000b880 && Address < 0x2000b8b0)
                Video.Write(Address, Value);
            else if (Address >= 0x20201000 && Address < 0x20201090)
                UART.Write(Address, Value);
            else if (Address >= 0x20215000 && Address < 0x20215080)
                MiniUART.Write(Address, Value);
        }

        /// <summary>
        ///     Writes 32-bits to Memory.
        /// </summary>
        /// <param name="Address">The Address to write the value into</param>
        /// <param name="Value">The value to be written</param>
        public void WriteUInt32(uint Address, uint Value)
        {
            WriteUInt8(Address, (byte)Value);
            WriteUInt8(Address + 1, (byte)(Value >> 8));
            WriteUInt8(Address + 2, (byte)(Value >> 16));
            WriteUInt8(Address + 3, (byte)(Value >> 24));
        }

        /// <summary>
        ///     Copy data from one region to another of the Memory.
        /// </summary>
        /// <param name="Source">The Source Address to start copying from</param>
        /// <param name="Destination">Where the data should be placed</param>
        /// <param name="Length">Number of bytes to copy</param>
        public void CopyData(uint Source, uint Destination, uint Length)
        {
            Buffer.BlockCopy(SDRAM, (int)Source, SDRAM, (int)Destination, (int)Length);
        }

        /// <summary>
        ///     Gets a Buffer at a given Address of the SDRAM.
        /// </summary>
        /// <param name="Address">The Address where the Buffer is located</param>
        /// <param name="Length">The length of the Buffer</param>
        /// <returns>The Buffer</returns>
        public byte[] GetData(uint Address, uint Length)
        {
            Address &= 0x3fffffff;

            byte[] Output = new byte[Length];
            Buffer.BlockCopy(SDRAM, (int)Address, Output, 0, Output.Length);
            return Output;
        }
    }
}
