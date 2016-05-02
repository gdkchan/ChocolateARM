using System;

namespace ChocolateARM.RPi.Peripherals
{
    /// <summary>
    ///     Represents the DMA control of the BCM2835 SoC.
    /// </summary>
    public class RPiDMA : IO32
    {
        private Memory Parent;

        /// <summary>
        ///     Creates a new instace of the RaspberryPi DMA manager.
        /// </summary>
        public RPiDMA(Memory Parent)
        {
            this.Parent = Parent;
            DMA = new DMA_Channel[16];
        }

        private struct DMA_Control
        {
            public bool IsActive;
            public bool IsFinished;
            public bool IsInterruptTriggered;

            /// <summary>
            ///     Loads the value of the DMA Control from a UInt32.
            /// </summary>
            /// <param name="Value">The UInt32 Control value</param>
            public void Set(uint Value)
            {
                IsActive = (Value & 1) != 0;
                if ((Value & 2) != 0) IsFinished = false;
                if ((Value & 4) != 0) IsInterruptTriggered = false;
            }

            /// <summary>
            ///     Creates a UInt32  from the values of the DMA Control.
            /// </summary>
            /// <returns>The Control value</returns>
            public uint Get()
            {
                uint Value = 0;
                if (IsActive) Value = 1;
                if (IsFinished) Value |= 2;
                if (IsInterruptTriggered) Value |= 4;
                if (!IsActive) Value |= 0x10;
                return Value;
            }
        }

        private struct DMA_Channel
        {
            public bool IsEnabled;

            public DMA_Control Control;
            public uint ControlBlockAddress;
        }
        private DMA_Channel[] DMA;

        /// <summary>
        ///     Occurs when a interrupt is triggered.
        /// </summary>
        public event Action OnInterruptRequest;

        /// <summary>
        ///     Reads a 32-bits value from the Register.
        /// </summary>
        /// <param name="Address">The Address being read</param>
        /// <returns>The value of the Register</returns>
        public override uint ProcessRead(uint Address)
        {
            uint Value = 0;
            
            if (Address >= 0x20007000 && Address < 0x20007f00)
                DMA_ProcessRead(Address, (Address >> 8) & 0xf);
            else if (Address == 0x20007ff0)
            {
                for (int Index = 0; Index < 16; Index++)
                {
                    if (DMA[Index].IsEnabled) Value |= (uint)(1 << Index);
                }
            }

            return Value;
        }

        /// <summary>
        ///     Writes a 32-bits value to a Register.
        /// </summary>
        /// <param name="Address">The Address being written</param>
        /// <param name="Value">The value being written</param>
        public override void ProcessWrite(uint Address, uint Value)
        {
            if (Address >= 0x20007000 && Address < 0x20007f00)
                DMA_ProcessWrite(Address, Value, (Address >> 8) & 0xf);
            else if (Address == 0x20007ff0)
            {
                for (int Index = 0; Index < 16; Index++)
                {
                    DMA[Index].IsEnabled = (Value & (1 << Index)) != 0;
                }
            }
        }

        /// <summary>
        ///     Reads a value from a DMA Channel.
        /// </summary>
        /// <param name="Address">The Address being read</param>
        /// <param name="Channel">The Channel being read</param>
        /// <returns>The value of the Channel register being read</returns>
        private uint DMA_ProcessRead(uint Address, uint Channel)
        {
            Address &= 0xff;

            switch (Address)
            {
                case 0: return DMA[Channel].Control.Get();
                case 4: return DMA[Channel].ControlBlockAddress;
                case 0x20:
                    uint Value = 0x4000000; //DMA Version (always 2)
                    if (Channel > 6) Value |= 0x10000000; //Lite
                    return Value;
            }

            return 0;
        }

        /// <summary>
        ///     Writes a value to a DMA Channel.
        /// </summary>
        /// <param name="Address">The Address being written</param>
        /// <param name="Value">The Value being written</param>
        /// <param name="Channel">The Channel being written</param>
        private void DMA_ProcessWrite(uint Address, uint Value, uint Channel)
        {
            Address &= 0xff;

            switch (Address)
            {
                case 0: DMA[Channel].Control.Set(Value); break;
                case 4: DMA[Channel].ControlBlockAddress = Value; break;
            }

            DMA_Transfer(Channel);
        }

        /// <summary>
        ///     Performs a DMA transfer (if any is pending).
        /// </summary>
        /// <param name="Channel">The Channel that is transferring data</param>
        private void DMA_Transfer(uint Channel)
        {
            if (DMA[Channel].IsEnabled && DMA[Channel].Control.IsActive)
            {
                while (DMA[Channel].ControlBlockAddress > 0)
                {
                    uint Address = DMA[Channel].ControlBlockAddress;

                    uint TransferInformation = Parent.ReadUInt32(Address);
                    uint SrcAddress = Parent.ReadUInt32(Address + 4);
                    uint DstAddress = Parent.ReadUInt32(Address + 8);
                    uint Length = Parent.ReadUInt32(Address + 12);
                    uint Stride = Parent.ReadUInt32(Address + 16);
                    uint NextAddress = Parent.ReadUInt32(Address + 20);

                    bool IsInterruptEnabled = (TransferInformation & 1) != 0;
                    bool Is2DModeEnabled = (TransferInformation & 2) != 0;

                    if (Channel < 7)
                    {
                        if (Is2DModeEnabled)
                        {
                            //2D Array transfer
                            short SrcStride = (short)Stride;
                            short DstStride = (short)(Stride >> 16);

                            ushort XLength = (ushort)Length;
                            ushort YLength = (ushort)(Length >> 16);

                            for (int Y = 0; Y < YLength; Y++)
                            {
                                Parent.CopyData(SrcAddress, DstAddress, XLength);

                                SrcAddress = (uint)(SrcAddress + XLength + SrcStride);
                                DstAddress = (uint)(DstAddress + XLength + DstStride);
                            }
                        }
                        else
                            Parent.CopyData(SrcAddress, DstAddress, Length); //Normal 1D transfer
                    }
                    else
                        Parent.CopyData(SrcAddress, DstAddress, (ushort)Length); //Lite, always 1D with 16-bits length

                    //Trigger interrupts if needed
                    if (IsInterruptEnabled)
                    {
                        if (OnInterruptRequest != null) OnInterruptRequest();
                        DMA[Channel].Control.IsInterruptTriggered = true;
                    }

                    DMA[Channel].ControlBlockAddress = NextAddress;
                }

                DMA[Channel].Control.IsActive = false;
                DMA[Channel].Control.IsFinished = true;
            }
        }
    }
}
