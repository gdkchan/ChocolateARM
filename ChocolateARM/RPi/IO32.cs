namespace ChocolateARM.RPi
{
    /// <summary>
    ///     "Converts" a sequence of 8-bits writes (or reads) into 32-bits values.
    ///     This class must be inherited, and the ProcessRead and ProcessWrite methods overloaded.
    /// </summary>
    public abstract class IO32
    {
        private uint ReadValue;
        private uint WriteValue;

        /// <summary>
        ///     Reads Data from the IO region.
        /// </summary>
        /// <param name="Address">The Address that is being read</param>
        /// <returns>The Data on the Address</returns>
        public byte Read(uint Address)
        {
            switch (Address & 3)
            {
                case 0:
                    ReadValue = ProcessRead(Address);
                    return (byte)ReadValue;
                case 1: return (byte)(ReadValue >> 8);
                case 2: return (byte)(ReadValue >> 16);
                case 3: return (byte)(ReadValue >> 24);
            }

            return 0;
        }

        /// <summary>
        ///     Writes Data to the IO region.
        /// </summary>
        /// <param name="Address">The Address where the Value is being written</param>
        /// <param name="Value">The Value that is being written</param>
        public void Write(uint Address, byte Value)
        {
            switch (Address & 3)
            {
                case 0: WriteValue = Value; break;
                case 1: WriteValue |= (uint)(Value << 8); break;
                case 2: WriteValue |= (uint)(Value << 16); break;
                case 3:
                    WriteValue |= (uint)(Value << 24);
                    ProcessWrite(Address & 0xfffffffc, WriteValue);
                    break;
            }
        }

        /// <summary>
        ///     This method is called whenever a new Read request has arrived.
        /// </summary>
        /// <param name="Address">The Address where the Read operation was made</param>
        /// <returns>The Data on the Address</returns>
        public abstract uint ProcessRead(uint Address);

        /// <summary>
        ///     This method is called whenever a new Write operation is made.
        /// </summary>
        /// <param name="Address">The Address where the Write operation was made</param>
        /// <param name="Value">The value being written</param>
        public abstract void ProcessWrite(uint Address, uint Value);
    }
}
