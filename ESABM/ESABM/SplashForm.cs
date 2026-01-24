using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ESABM {
    public partial class SplashForm:Form {
        Timer fadeTimer = new Timer();
        bool fadingIn = true;
        int progress = 0;
        public SplashForm() {
            InitializeComponent();
        }

        private void axWindowsMediaPlayer1_Enter(object sender,EventArgs e) {

        }

        private void SplashForm_Load(object sender,EventArgs e) {
            this.Opacity=0;

            fadeTimer.Interval=30;
            fadeTimer.Tick+=FadeTimer_Tick;
            fadeTimer.Start();

            axWindowsMediaPlayer1.Width=618;
            axWindowsMediaPlayer1.Height=360;
            axWindowsMediaPlayer1.stretchToFit=true;
            axWindowsMediaPlayer1.uiMode="none";
            axWindowsMediaPlayer1.settings.autoStart=true;
            axWindowsMediaPlayer1.settings.volume=100;

            string videoPath = Path.Combine(
                Application.StartupPath,
                "Files",
                "Untitled design.mp4"
            );
            if(!File.Exists(videoPath)) {
                MessageBox.Show("Video file missing:\n"+videoPath);
                return;
            }

            axWindowsMediaPlayer1.URL=videoPath;
            axWindowsMediaPlayer1.Ctlcontrols.play();

            axWindowsMediaPlayer1.PlayStateChange+=(s,ev) => {
                if(ev.newState==8)
                {
                    fadingIn=false;
                    fadeTimer.Start();
                }
            };

            progressBar1.BringToFront();
        }
        private void FadeTimer_Tick(object sender,EventArgs e) {
            if(fadingIn) {
                if(this.Opacity<1)
                    this.Opacity+=0.05;
                if(progress<100) {
                    progress++;
                    progressBar1.Value=progress;
                    label2.Text=$"Loading- {progress}%";
                }
                else {
                    fadeTimer.Stop();
                }
            }
            else {
                if(this.Opacity>0)
                    this.Opacity-=0.05;
                else {
                    fadeTimer.Stop();this.Close();
                }
            }
        }
        private void label1_Click(object sender,EventArgs e) {

        }

        private void button3_Click(object sender,EventArgs e) {
            Environment.Exit(0);
        }
    }
}
