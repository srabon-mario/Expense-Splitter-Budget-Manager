using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ESABM {
    public class AppContext:ApplicationContext {
        public AppContext() {
            SplashForm splash = new SplashForm();
            splash.Show();

            splash.FormClosed+=(s,e) => {
                Form1 main = new Form1();
                main.FormClosed+=(s2,e2) => ExitThread();
                main.Show();
            };
        }
    }
}
