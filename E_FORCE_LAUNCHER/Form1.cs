using System;
using System.Media;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using E_FORCE_LAUNCHER.DiscordRpcDemo;
using System.Windows.Forms.DataVisualization.Charting;

namespace E_FORCE_LAUNCHER
{
    public partial class Form1 : Form
    {
        private DiscordRpc.EventHandlers handlers;
        private DiscordRpc.RichPresence presence;

        string current_version = "beta1.9";
        bool eforceupdater = false;
        int lastdownlaodid;
        bool version_seted_to_td = false;

        // -> MYSQL Connect
        string connectionString = string.Format(
            "server={0};uid={1};pwd={2};database={3}",
            "188.214.88.82",
            "root",
            "!#SSdEUqh4U3JZ@",
            "eforce"
        );

        string ftp_user = "essrocom";
        string ftp_password = "iHq4as0K16";
        string ftp_download = "ftp://e-force.ro/public_html/launcher.e-force.ro/files/";

        int mov;
        int movX;
        int movY;

        void DownlaodFTPToLocal(string app, string appto, bool for_eforce_updater = false, bool starteforceautomatically = false)
        {
            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(ftp_user, ftp_password);
                byte[] fileData = request.DownloadData(ftp_download + app);
                progressBar1.Show();

                using (FileStream file = File.Create(appto))
                {
                    FileInfo fi = new FileInfo(appto);
                    long size = fi.Length;
                    if (fileData.Length == size)
                    {
                        progressBar1.Hide();
                        if (starteforceautomatically == true)
                        {
                            button1.Text = "Play";
                            button1.Show();
                            StartEForce();
                        }
                    }
                    else
                    {
                        file.Write(fileData, 0, fileData.Length);
                        if (for_eforce_updater == false)
                        {
                            if (starteforceautomatically == true)
                            {
                                button1.Text = "Play";
                                button1.Show();
                                StartEForce();
                            }
                        }
                        else if (for_eforce_updater == true)
                        {
                            Process.Start("eforce.exe");
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                    }
                    file.Close();
                }
            }
        }
        void DownloadFileByID(int id)
        {
            // -> Create directories if not exist
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files"))) Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files"));

            // -> Update download 
            lastdownlaodid = 7;
            if (id == 1) Download(1, "samp.exe", "samp.exe", "samp.exe");
            else if (id == 2) Download(2, "mods/Documents/GTA San Andreas User Files/gta_sa.set", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files/gta_sa.set"), "gta_sa.set");
            else if (id == 3) Download(3, "mods/models/hud.txd", "models/hud.txd", "hud.txd");
            else if (id == 4) Download(4, "mods/data/maps/empty.ipl", "data/maps/empty.ipl", "empty.ipl");
            else if (id == 5) Download(5, "mods/CustomSAA2/gta.dat", "CustomSAA2/gta.dat", "gta.dat");
            else if (id == 6) Download(6, "mods/models/eforce.img", "models/eforce.img", "eforce.img");
            else if (id == 7) Download(7, "mods/fastman92limitAdjuster_GTASA.ini", "fastman92limitAdjuster_GTASA.ini", "fastman92limitAdjuster_GTASA.ini");
            else if (id == 8) Download(8, "mods/models/gta3.img", "models/gta3.img", "gta3.img");
        }
        private void Download(int count, string app, string appto, string withextension)
        {
            NetworkCredential credentials = new NetworkCredential(ftp_user, ftp_password);

            // Query size of the file to be downloaded
            WebRequest sizeRequest = WebRequest.Create(ftp_download + app);
            sizeRequest.Credentials = credentials;
            sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;

            int size = (int)sizeRequest.GetResponse().ContentLength;

            progressBar1.Invoke((MethodInvoker)(() => progressBar1.Maximum = size));

            // Download the file
            WebRequest request = WebRequest.Create(ftp_download + app);
            request.Credentials = credentials;
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            void DownloadFileBeingVerified()
            {
                progressBar1.Show();
                label10.Text = withextension;
                label9.Show();
                label10.Show();

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(appto))
                {
                    byte[] buffer = new byte[10240];
                    int read;
                    int finished_task = 0;
                    while (finished_task == 0)
                    {
                        read = ftpStream.Read(buffer, 0, buffer.Length);

                        int position = (int)fileStream.Position;
                        if (position == size)
                        {
                            if (count == lastdownlaodid)
                            {
                                button1.Text = "Play";
                                progressBar1.Hide();
                                label9.Hide();
                                label10.Hide();
                                button1.Show();
                                StartEForce();
                                finished_task = 1;
                            }
                            else
                            {
                                finished_task = 1;
                                DownloadFileByID(count + 1);
                            }
                        }
                        else
                        {
                            fileStream.Write(buffer, 0, read);
                            progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = position));
                        }
                    }
                }
            }

            try
            {
                FileInfo f = new FileInfo(appto);
                int currentsize = Convert.ToInt32(f.Length);

                if (currentsize == size)
                {
                    if (count == lastdownlaodid)
                    {
                        button1.Text = "Play";
                        button1.Show();
                        progressBar1.Hide();
                        label9.Hide();
                        label10.Hide();
                        StartEForce();
                    }
                    else DownloadFileByID(count + 1);
                }
                else DownloadFileBeingVerified();
            }
            catch
            {
                DownloadFileBeingVerified();
            }
        }

        void LoadDiscordRCP()
        {
            try
            {
                // -> Discord rich presence
                this.handlers = default(DiscordRpc.EventHandlers);
                DiscordRpc.Initialize("757606218717069343", ref this.handlers, true, null);
                this.handlers = default(DiscordRpc.EventHandlers);
                DiscordRpc.Initialize("C757606218717069343", ref this.handlers, true, null);
                this.presence.details = "E-FORCE ROMANIA MULTIPLAYER";
                this.presence.state = "www.e-force.ro";
                this.presence.largeImageKey = "logo";
                //this.presence.smallImageKey = "eforce_logo";
                this.presence.largeImageText = "E-Force Romania Multi-Player";
                DiscordRpc.UpdatePresence(ref this.presence);
            }
            catch
            {
                DownlaodFTPToLocal("discord-rpc-w32.dll", "discord-rpc-w32.dll");
                LoadDiscordRCP();
            }
        }
        void RefreshDetails()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM `users` WHERE `status` = '1'", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            label6.Text = string.Format("{0}/1000", reader.GetInt16(0));
                        }
                    }
                    reader.Close();
                }
                using (MySqlCommand cmd = new MySqlCommand("SELECT status FROM settings;", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt16(0) == 0) { label8.Text = "Offline"; label8.ForeColor = Color.Red; }
                            else if (reader.GetInt16(0) == 1) { label8.Text = "Online"; label8.ForeColor = Color.LimeGreen; }
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
        }

        public Form1()
        {
            InitializeComponent();

           /* // Stop multiple app open
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) System.Diagnostics.Process.GetCurrentProcess().Kill();

            // -> Create launcher_settings Folder if not exist
            string launcher_settings_path = "launcher_settings";

            if (!Directory.Exists(launcher_settings_path))
            {
                Directory.CreateDirectory(launcher_settings_path);
            }


            LoadDiscordRCP();
            RefreshDetails();

            // -> Timer for Checkins
            Timer CheckingApps = new Timer();
            CheckingApps.Interval = 1000;
            CheckingApps.Tick += new EventHandler(CheckingApps_Tick);
            CheckingApps.Start();

            // -> Read last name
            string name_save = "launcher_settings/lastname.txt";
            try
            {
                string text = System.IO.File.ReadAllText(name_save);
                textBox1.Text = text;
            }
            catch
            {
                try
                {
                    File.CreateText(name_save);
                }
                catch { }
            }

            // -> EFORCE Updater .exe
            DownlaodFTPToLocal("eforceupdater.exe", "eforceupdater.exe");

            progressBar1.Hide();
            label9.Hide();
            label10.Hide();
            label12.Hide();

            foreach (var process in Process.GetProcessesByName("gta_sa")) { process.Kill(); }*/
        }

        /*void StartEForce()
        {
            if (!File.Exists("samp.dll")) { MessageBox.Show("samp.dll not found!"); }
            else
            {
                if (File.Exists("d3d9.dll")) { File.Delete("d3d9.dll"); MessageBox.Show("Restricted file have been detected!\nDeleting restricted file..."); }
                if (textBox1.Text == "") { MessageBox.Show("Please insert a valid name!"); }
                else
                {
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        using (MySqlCommand cmd = new MySqlCommand("SELECT IP FROM settings;", conn))
                        {
                            conn.Open();
                            MySqlDataReader reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) == "")
                                    {
                                        MessageBox.Show("No IP available right now!");
                                    }
                                    else
                                    {
                                        Process.Start("samp.exe", string.Format("-c -n {0} -h {1} -p 7777 -z qwerty1337", textBox1.Text, reader.GetString(0)));
                                    }
                                }
                            }
                            reader.Close();
                        }
                    }
                }
            }
        }
        private void CheckingApps_Tick(object sender, EventArgs e)
        {
            if (eforceupdater == true) DownlaodFTPToLocal("eforce.exe", "eforce.exe", true);
            else
            {
                // -> Hide Play button if the client have gta_sa.exe opened
                Process[] pname1 = Process.GetProcessesByName("gta_sa");
                if (pname1.Length > 0) { button1.Hide(); }
                else { button1.Show(); }

                try
                {
                    System.IO.File.WriteAllText("launcher_settings/lastname.txt", textBox1.Text);
                }
                catch { }

                // -> Auto Launcher Updater
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT launcher_version FROM settings;", conn))
                    {
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (version_seted_to_td == false)
                                {
                                    label12.Text = reader.GetString(0);
                                    label12.Show();
                                    version_seted_to_td = true;
                                }
                                if (reader.GetString(0) != current_version)
                                {
                                    if (File.Exists("eforceupdater.exe"))
                                    {
                                        foreach (var process in Process.GetProcessesByName("gta_sa")) { process.Kill(); }
                                        Process.Start("eforceupdater.exe");
                                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                                    }
                                    else { DownlaodFTPToLocal("eforceupdater.exe", "eforceupdater.exe"); }
                                }
                            }
                        }
                        reader.Close();
                    }
                }
            }
        }*/

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //button1.Text = "Wait...";
            //if (File.Exists("samp-discord-plugin.asi")) { File.Delete("samp-discord-plugin.asi"); }
            //DownloadFileByID(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           // RefreshDetails();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            //System.Windows.Forms.Application.Exit();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            //mov = 1;
            //movX = e.X-50;
            //movY = e.Y;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //if(mov == 1)
            //{
            //    this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
           // }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
           // mov = 0;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            //Process.Start("http://www.e-force.ro/rpg");
        }

        private void label3_Click(object sender, EventArgs e)
        {
            //Process.Start("http://www.e-force.ro/forums");
        }

        private void label4_Click(object sender, EventArgs e)
        {
            //Process.Start("http://www.e-force.ro/discord");
        }

        private void label2_MouseHover_1(object sender, EventArgs e)
        {
            //label2.ForeColor = Color.Brown;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            //label2.ForeColor = Color.Transparent;
        }

        private void label3_MouseHover(object sender, EventArgs e)
        {
            //label3.ForeColor = Color.Brown;
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            //label3.ForeColor = Color.Transparent;
        }

        private void label4_MouseHover(object sender, EventArgs e)
        {
            //label4.ForeColor = Color.Brown;
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            //label4.ForeColor = Color.Transparent;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_MouseHover(object sender, EventArgs e)
        {
            pictureBox5.BackColor = Color.MidnightBlue;
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.BackColor = Color.Transparent;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            //Process.Start("https://www.facebook.com/E-Force-Romania-259807871375550");
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
            button1.BackColor = Color.Lime;
            button1.ForeColor = Color.Black;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.Green;
            button1.ForeColor = Color.White;
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.Brown;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.Transparent;
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.Tan;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.Transparent;
        }
    }
}
