using System;
using System.Windows.Forms;

using ChocolateARM.RPi;
using ChocolateARM.RPi.Peripherals;

namespace ChocolateARM
{
    public partial class FrmUARTOutput : Form
    {
        RPiCore RaspberryPi;

        public FrmUARTOutput(RPiCore RaspberryPi)
        {
            this.RaspberryPi = RaspberryPi;

            InitializeComponent();
        }

        private void FrmUARTOutput_Load(object sender, EventArgs e)
        {
            Show();

            CharacterReceived = new CharacterReceivedDelegate(ReceiveCharacter);

            //Update texts
            TxtMiniUART.AppendText(RaspberryPi.Memory.MiniUART.BufferedOutput ?? "");
            TxtUART.AppendText(RaspberryPi.Memory.UART.BufferedOutput ?? "");

            //And subscribe for new characters
            RaspberryPi.Memory.MiniUART.OnCharacterReceived += MU_CharacterReceived;
            RaspberryPi.Memory.UART.OnCharacterReceived += UART_CharacterReceived;
        }

        private CharacterReceivedDelegate CharacterReceived;
        private delegate void CharacterReceivedDelegate(TextBox Target, char Character);

        private void MU_CharacterReceived(object sender, CharacterReceivedEventArgs e)
        {
            BeginInvoke(CharacterReceived, TxtMiniUART, e.Character);
        }

        private void UART_CharacterReceived(object sender, CharacterReceivedEventArgs e)
        {
            BeginInvoke(CharacterReceived, TxtUART, e.Character);
        }

        private void ReceiveCharacter(TextBox Target, char Character)
        {
            Target.AppendText(Character.ToString());
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (Tabs.SelectedIndex == 0)
                TxtMiniUART.Clear();
            else
                TxtUART.Clear();
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            TxtMiniUART.Clear();
            TxtUART.Clear();
        }
    }
}
