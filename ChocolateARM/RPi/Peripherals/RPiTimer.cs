using System;
using System.Timers;

namespace ChocolateARM.RPi.Peripherals
{
    /// <summary>
    ///     Represents the Timer of the BCM2835 SoC.
    /// </summary>
    public class RPiTimer : IO32, IDisposable
    {
        const double ARMT_Frequency = 1000000; //1MHz

        private uint ARMT_LoadValue;
        private uint ARMT_Value;
        private uint ARMT_Control;
        private uint ARMT_Reload;
        private uint ARMT_PreDivider;
        private uint ARMT_FreeRunCounter;

        private Timer Countdown;
        private Timer FreeRun;

        /// <summary>
        ///     Occurs when a interrupt is triggered.
        /// </summary>
        public event Action OnInterruptRequest;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Countdown.Dispose();
                FreeRun.Dispose();
            }
        }

        /// <summary>
        ///     Creates a new instace of the RaspberryPi Timer.
        /// </summary>
        public RPiTimer()
        {
            ARMT_Control = 0x3e0020;
            ARMT_PreDivider = 0x7d;

            Countdown = new Timer();
            FreeRun = new Timer();

            Countdown.Elapsed += Countdown_Tick;
            FreeRun.Elapsed += FreeRun_Tick;
        }

        /// <summary>
        ///     Reads a value from the Timer registers.
        /// </summary>
        /// <param name="Address">The Address of the read</param>
        /// <returns>The value at the Address</returns>
        public override uint ProcessRead(uint Address)
        {
            switch (Address)
            {
                case 0x2000b400: return ARMT_LoadValue;
                case 0x2000b404:
                    if ((ARMT_Control & 2) != 0)
                        ARMT_Value &= 0x7fffff;
                    else
                        ARMT_Value &= 0xffff;
                    return ARMT_Value;
                case 0x2000b408: return ARMT_Control;
                case 0x2000b40c: return 0x544d5241;
                case 0x2000b410: return 0x3e0020;
                case 0x2000b414: return 0x3e0020;
                case 0x2000b418: return ARMT_Reload;
                case 0x2000b41c: return ARMT_PreDivider;
                case 0x2000b420: return ARMT_FreeRunCounter;
            }

            return 0;
        }

        /// <summary>
        ///     Writes a value to the Timer registers.
        /// </summary>
        /// <param name="Address">The Address of the write</param>
        /// <param name="Value">The value being written</param>
        public override void ProcessWrite(uint Address, uint Value)
        {
            switch (Address)
            {
                case 0x2000b400:
                    ARMT_LoadValue = Value;
                    ARMT_Value = Value;
                    break;
                case 0x2000b408:
                    ARMT_Control = Value;
                    switch ((Value >> 2) & 3)
                    {
                        case 1: Countdown.Interval = 1000d / (ARMT_Frequency / 16); break;
                        case 2: Countdown.Interval = 1000d / (ARMT_Frequency / 256); break;
                        default: Countdown.Interval = 1000d / ARMT_Frequency; break;
                    }
                    Countdown.Enabled = (Value & 0x80) != 0;
                    FreeRun.Enabled = (Value & 0x200) != 0;
                    FreeRun.Interval = 1000d / (ARMT_Frequency / (((Value >> 16) & 0xff) + 1));
                    break;
                case 0x2000b418: ARMT_Reload = Value; break;
                case 0x2000b41c: ARMT_PreDivider = Value; break;
                case 0x2000b420: ARMT_FreeRunCounter = Value; break;
            }
        }

        private void Countdown_Tick(object sender, ElapsedEventArgs e)
        {
            if (ARMT_Value > 0)
                ARMT_Value--;
            else
            {
                ARMT_Value = ARMT_LoadValue;
                bool IsInterruptEnabled = (ARMT_Control & 0x20) != 0;
                if (IsInterruptEnabled && OnInterruptRequest != null) OnInterruptRequest();
            }
        }

        private void FreeRun_Tick(object sender, ElapsedEventArgs e)
        {
            ARMT_FreeRunCounter++;
        }
    }
}
