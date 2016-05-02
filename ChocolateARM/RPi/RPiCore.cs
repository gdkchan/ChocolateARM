using System.IO;
using System.Threading;

using SharpARM.ARM11;

namespace ChocolateARM.RPi
{
    /// <summary>
    ///     Represents a Raspberry Pi (version 1).
    /// </summary>
    public class RPiCore
    {
        public ARMCore CPU;
        public Memory Memory;

        private bool IsExecuting;

        /// <summary>
        ///     Creates a new instance of the Raspberry Pi emulator.
        /// </summary>
        /// <param name="Screen">The Picture Box to draw the video output</param>
        public RPiCore()
        {
            Memory = new Memory();
            CPU = new ARMCore(Memory);
        }

        /// <summary>
        ///     Loads a binary file into Memory.
        /// </summary>
        /// <param name="FileName">The full path to the file that should be loaded</param>
        /// <param name="Address">The Address to place the file into Memory</param>
        /// <param name="SetPC">Set the value of PC to the value of Address (where the binary is loaded)</param>
        public void Load(string FileName, uint Address, bool SetPC = false)
        {
            byte[] Buffer = File.ReadAllBytes(FileName);
            for (uint i = 0; i < Buffer.Length; i++)
            {
                Memory.WriteUInt8(Address + i, Buffer[i]);
            }

            CPU.Reset();
            if (SetPC)
            {
                CPU.Registers[15] = Address;
                CPU.ReloadPipeline();
            }
        }

        /// <summary>
        ///     Executes the program asynchronously until Stop is called.
        /// </summary>
        public void RunAsync()
        {
            Thread ExecutionThread = new Thread(Run);
            ExecutionThread.Start();
        }

        /// <summary>
        ///     Executes instruction on a loop until Stop is called.
        /// </summary>
        private void Run()
        {
            IsExecuting = true;
            while (IsExecuting) CPU.Execute();
        }

        /// <summary>
        ///     Stops executing the instructions.
        /// </summary>
        public void Stop()
        {
            IsExecuting = false;
        }
    }
}
