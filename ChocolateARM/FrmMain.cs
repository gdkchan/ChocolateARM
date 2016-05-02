using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ChocolateARM.RPi;

namespace ChocolateARM
{
    public partial class FrmMain : Form
    {
        RPiCore RaspberryPi;

        public FrmMain()
        {
            InitializeComponent();
        }

        private int ScreenDifferenceWidth;
        private int ScreenDifferenceHeight;

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ScreenDifferenceWidth = Width - Screen.Width;
            ScreenDifferenceHeight = Height - Screen.Height;

            RaspberryPi = new RPiCore();
            RaspberryPi.Memory.Video.OnScreenSetup += ScreenSetupCallback;
            SetupScreen = new ScreenSetupDelegate(ScreenSetup);

            Show();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            RaspberryPi.Stop();
        }

        private void ScreenSetupCallback()
        {
            BeginInvoke(SetupScreen);
        }

        private ScreenSetupDelegate SetupScreen;
        private delegate void ScreenSetupDelegate();

        private void ScreenSetup()
        {
            Screen.Width = (int)RaspberryPi.Memory.Video.ScreenVirtualWidth;
            Screen.Height = (int)RaspberryPi.Memory.Video.ScreenVirtualHeight;
            Width = Screen.Width + ScreenDifferenceWidth;
            Height = Screen.Height + ScreenDifferenceHeight;
            CenterToScreen();
            ScreenRefresh.Enabled = true;
        }

        private void MnuLoadKernelImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenDialog = new OpenFileDialog())
            {
                OpenDialog.Title = "Load Kernel.img";
                OpenDialog.Filter = "Image|*.img";

                if (OpenDialog.ShowDialog() == DialogResult.OK && File.Exists(OpenDialog.FileName))
                {
                    RaspberryPi.Load(OpenDialog.FileName, 0x8000, true);
                    RaspberryPi.RunAsync();
                }
            }
        }

        private void MnuDumpSDRAM_Click(object sender, EventArgs e)
        {
            File.WriteAllBytes(@"D:\pimem.bin", RaspberryPi.Memory.SDRAM);
        }

        private void ScreenRefresh_Tick(object sender, EventArgs e)
        {
            Bitmap Img = RaspberryPi.Memory.Video.GetImage();
            if (Img != null) Screen.Image = Img;
        }

        private void MnuShowUART_Click(object sender, EventArgs e)
        {
            FrmUARTOutput Form = new FrmUARTOutput(RaspberryPi);
            Form.Show();
        }

        private void MnuARMDbg_Click(object sender, EventArgs e)
        {
            RaspberryPi.CPU.Debug = !RaspberryPi.CPU.Debug;
        }
    }
}
