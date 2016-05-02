using System;
using System.Collections.Generic;

namespace ChocolateARM.RPi.Peripherals
{
    /// <summary>
    ///     Represents a new character that arrived at the UART.
    /// </summary>
    public class CharacterReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     The new character that arrived on the UART.
        /// </summary>
        public char Character;

        /// <summary>
        ///     Creates a new Argument with the character written to the UART.
        /// </summary>
        /// <param name="Character">The new character</param>
        public CharacterReceivedEventArgs(char Character)
        {
            this.Character = Character;
        }
    }

    /// <summary>
    ///     Represents the Mini UART of the BCM2835 SoC.
    /// </summary>
    public class RPiMiniUART : IO32
    {
        private const uint AUX_ENB = 0x20215004;
        private const uint AUX_MU_IO_REG = 0x20215040;
        private const uint AUX_MU_IIR_REG = 0x20215044;
        private const uint AUX_MU_IER_REG = 0x20215048;
        private const uint AUX_MU_LCR_REG = 0x2021504c;
        private const uint AUX_MU_MCR_REG = 0x20215050;
        private const uint AUX_MU_LSR_REG = 0x20215054;
        private const uint AUX_MU_MSR_REG = 0x20215058;
        private const uint AUX_MU_SCRATCH = 0x2021505c;
        private const uint AUX_MU_CNTL_REG = 0x20215060;
        private const uint AUX_MU_STAT_REG = 0x20215064;
        private const uint AUX_MU_BAUD_REG = 0x20215068;

        private Memory Parent;

        private Queue<byte> Transmitter;
        private Queue<byte> Receiver;

        /// <summary>
        ///     Creates a new instace of the RaspberryPi Mini UART.
        /// </summary>
        public RPiMiniUART(Memory Parent)
        {
            this.Parent = Parent;
            Transmitter = new Queue<byte>();
            Receiver = new Queue<byte>();

            MU_IsReceiverEnabled = true;
            MU_IsTransmitterEnabled = true;
        }

        private bool MU_IsEnabled;
        private bool SPI1_IsEnabled;
        private bool SPI2_IsEnabled;

        private bool MU_IsTransmitInterruptEnabled;
        private bool MU_IsReceiveInterruptEnabled;
        private bool MU_8BitsMode;
        private bool MU_DLAB;
        private bool MU_ReceiverOverrun;
        private byte MU_Scratch;
        private bool MU_IsReceiverEnabled;
        private bool MU_IsTransmitterEnabled;
        private ushort MU_BaudRate;

        /// <summary>
        ///     Occurs when a interrupt is triggered.
        /// </summary>
        public event Action OnInterruptRequest;

        /// <summary>
        ///     Occurs whenever a new character is written to the UART.
        /// </summary>
        public event EventHandler<CharacterReceivedEventArgs> OnCharacterReceived;

        /// <summary>
        ///     Contains a copy of all data written to the UART.
        /// </summary>
        public string BufferedOutput { get; private set; }

        /// <summary>
        ///     Reads a 32-bits value from the Register.
        /// </summary>
        /// <param name="Address">The Address being read</param>
        /// <returns>The value of the Register</returns>
        public override uint ProcessRead(uint Address)
        {
            uint Value = 0;

            switch (Address)
            {
                case AUX_ENB:
                    if (MU_IsEnabled) Value = 1;
                    if (SPI1_IsEnabled) Value |= 2;
                    if (SPI2_IsEnabled) Value |= 4;
                    break;

                case AUX_MU_IO_REG:
                    if (MU_DLAB)
                        Value = (uint)(MU_BaudRate & 0xff);
                    else if (Receiver.Count > 0)
                        Value = Receiver.Dequeue();
                    break;

                case AUX_MU_IIR_REG:
                    if (MU_DLAB)
                        Value = (uint)(MU_BaudRate >> 8);
                    else
                    {
                        if (MU_IsReceiveInterruptEnabled) Value = 1;
                        if (MU_IsTransmitInterruptEnabled) Value |= 2;
                    }
                    break;

                case AUX_MU_IER_REG:
                    Value = 0xc1;
                    if (Transmitter.Count == 0) Value |= 2;
                    if (Receiver.Count > 0) Value |= 4;
                    break;

                case AUX_MU_LCR_REG:
                    if (MU_8BitsMode) Value = 1;
                    if (MU_DLAB) Value |= 0x80;
                    break;

                case AUX_MU_LSR_REG:
                    if (Receiver.Count > 0) Value = 1;
                    if (MU_ReceiverOverrun)
                    {
                        Value |= 2;
                        MU_ReceiverOverrun = false;
                    }
                    if (Transmitter.Count < 8) Value |= 0x20;
                    if (Transmitter.Count == 0) Value |= 0x40;
                    break;

                case AUX_MU_SCRATCH: Value = MU_Scratch; break;

                case AUX_MU_CNTL_REG:
                    if (MU_IsReceiverEnabled) Value = 1;
                    if (MU_IsTransmitterEnabled) Value |= 2;
                    break;

                case AUX_MU_STAT_REG:
                    Value = 0xc;
                    if (Receiver.Count > 0) Value |= 1;
                    if (Transmitter.Count < 8) Value |= 2;
                    if (MU_ReceiverOverrun) Value |= 0x10;
                    if (Transmitter.Count == 8) Value |= 0x20;
                    if (Transmitter.Count == 0) Value |= 0x300;
                    Value |= (uint)((Receiver.Count & 0xf) << 16);
                    Value |= (uint)((Transmitter.Count & 0xf) << 24);
                    break;

                case AUX_MU_BAUD_REG: Value = MU_BaudRate; break;
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
            if (Address >= 0x20215040 && !MU_IsEnabled) return;

            switch (Address)
            {
                case AUX_ENB:
                    MU_IsEnabled = (Value & 1) != 0;
                    SPI1_IsEnabled = (Value & 2) != 0;
                    SPI2_IsEnabled = (Value & 4) != 0;
                    break;

                case AUX_MU_IO_REG:
                    if (MU_DLAB)
                        MU_BaudRate = (ushort)((MU_BaudRate & 0xff00) | (byte)Value);
                    else if (Transmitter.Count < 8 && MU_IsTransmitterEnabled)
                    {
                        Transmitter.Enqueue((byte)Value);
                        AppendCharacter();
                    }
                    break;

                case AUX_MU_IIR_REG:
                    if (MU_DLAB)
                        MU_BaudRate = (ushort)((MU_BaudRate & 0xff) | ((byte)Value << 8));
                    else
                    {
                        MU_IsReceiveInterruptEnabled = (Value & 1) != 0;
                        MU_IsTransmitInterruptEnabled = (Value & 2) != 0;
                    }
                    break;

                case AUX_MU_IER_REG:
                    if ((Value & 2) != 0) Receiver.Clear();
                    if ((Value & 4) != 0) Transmitter.Clear();
                    break;

                case AUX_MU_LCR_REG:
                    MU_8BitsMode = (Value & 1) != 0;
                    MU_DLAB = (Value & 0x80) != 0;
                    break;

                case AUX_MU_SCRATCH: MU_Scratch = (byte)Value; break;

                case AUX_MU_CNTL_REG:
                    MU_IsReceiverEnabled = (Value & 1) != 0;
                    MU_IsTransmitterEnabled = (Value & 2) != 0;
                    break;

                case AUX_MU_BAUD_REG: MU_BaudRate = (ushort)Value; break;
            }
        }

        /// <summary>
        ///     Appends a character on the console.
        /// </summary>
        private void AppendCharacter()
        {
            char Character = (char)Transmitter.Dequeue();
            if (OnCharacterReceived != null) OnCharacterReceived(this, new CharacterReceivedEventArgs(Character));
            BufferedOutput += Character;

            bool TriggerInterrupt = Transmitter.Count == 0 && MU_IsTransmitInterruptEnabled;
            if (TriggerInterrupt && OnInterruptRequest != null) OnInterruptRequest();
        }
    }
}
