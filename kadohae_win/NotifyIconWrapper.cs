using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 角から生えるやつ
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            InitializeComponent();

            // イベントハンドラの設定
            toolStripMenuItemQuit.Click += toolStripMenuItemQuit_Click;
        }

        void toolStripMenuItemQuit_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
