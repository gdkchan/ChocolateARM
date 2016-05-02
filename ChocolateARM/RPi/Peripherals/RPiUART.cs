using System;
using System.Collections.Generic;

namespace ChocolateARM.RPi.Peripherals
{
    /// <summary>
    ///     Represents the UART (PL011) of the BCM2835 SoC.
    /// </summary>
    public class RPiUART : IO32
    {
        private Queue<byte> Transmitter;
        private Queue<byte> Receiver;

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

        public RPiUART()
        {
            Transmitter = new Queue<byte>();
            Receiver = new Queue<byte>();
        }

        /// <summary>
        ///     Reads a 32-bits value from the Register.
        /// </summary>
        /// <param name="Address">The Address being read</param>
        /// <returns>The value of the Register</returns>
        public override uint ProcessRead(uint Address)
        {
            switch (Address)
            {
                case 0x20201000: if (Receiver.Count > 0) return Receiver.Dequeue(); break;
                case 0x20201018:
                    uint Value = 0;
                    if (Receiver.Count == 0) Value = 0x10;
                    if (Transmitter.Count == 8) Value |= 0x20;
                    if (Receiver.Count == 8) Value |= 0x40;
                    if (Transmitter.Count == 0) Value |= 0x80;
                    return Value;
            }

            return 0;
        }

        /// <summary>
        ///     Writes a 32-bits value to a Register.
        /// </summary>
        /// <param name="Address">The Address being written</param>
        /// <param name="Value">The value being written</param>
        public override void ProcessWrite(uint Address, uint Value)
        {
            switch (Address)
            {
                case 0x20201000:
                    if (Transmitter.Count < 8)
                    {
                        Transmitter.Enqueue((byte)Value);
                        AppendCharacter();
                    }
                    break;
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
        }
    }
}
