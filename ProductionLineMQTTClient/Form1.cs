using LiveCharts;
using LiveCharts.Wpf;
using MQTTnet;
using MQTTnet.Client;
using System.IO;
using System.Text;
using System.Windows.Forms.Integration;

namespace ProductionLineMQTTClient 
{
    public partial class Form1 : Form 
    {
        private IMqttClient mqttClient;
        private System.Windows.Forms.Label lblProductionStatus;
        private Dictionary<string, Label> machineLabels = new Dictionary<string, Label>();

        private System.Windows.Forms.Timer alertTimer;
        private DateTime lastStopTime;
        private TimeSpan downtimeDuration;
        private DateTime lineStartTime;

        private TimeSpan totalUptime = TimeSpan.Zero;
        private TimeSpan totalDowntime = TimeSpan.Zero;
        private TimeSpan statusDuration;
        private DateTime lastStatusChangeTime = default(DateTime);

        private double lineCounter = 0;
        private double wasteCounter = 0;
        private double targetCounter = 0;

        private Label closeButton;
        private PictureBox successIcon;
        private PictureBox errorIcon;

        private FlowLayoutPanel panelCards;

        public enum IconType 
        {
            Availability,
            Performance,
            Quality
        }

        public Form1() 
        {
            InitializeComponent();

            panelCards = new FlowLayoutPanel 
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            Controls.Add(panelCards);

            machineLabels.Add("M1", label1);
            machineLabels.Add("M2", label2);
            machineLabels.Add("M3", label3);
            machineLabels.Add("M4", label4);
            machineLabels.Add("M5", label5);

            this.Load += new EventHandler(MainForm_Load);
        }

        private void MainForm_Load(object sender, EventArgs e) 
        {
            InitializeLabels();
            InitializeIcons();
            ConnectMQTT();
        }

        private void InitializeLabels() 
        {
            label6.Text = "Production Status: Unknown";
            label7.Text = "Production Counter: 0";
            label8.Text = "Waste Counter: 0";
            label9.Text = "Target: 0";
            label12.Text = "Robot System Automation is a dynamic manufacturing company founded in 1984 with the goal of building \ninnovative, automated production systems for the footwear industry.\nWe have extensive experience in this highly competitive field. Experience, combined with the desire to \ncontinually improve our products, services and processes, has made Robot System Automation's state of \nthe art production facility a world leader both in the study of new technologies and in the development of \nnew concepts for the application of robots in the footwear industry.";
            label14.Text = "Production";

            label1.Font = new Font("Constantia", 10, FontStyle.Regular);
            label2.Font = new Font("Constantia", 10, FontStyle.Regular);
            label3.Font = new Font("Constantia", 10, FontStyle.Regular);
            label4.Font = new Font("Constantia", 10, FontStyle.Regular);
            label5.Font = new Font("Constantia", 10, FontStyle.Regular);
            label6.Font = new Font("Constantia", 10, FontStyle.Regular);
            label7.Font = new Font("Constantia", 10, FontStyle.Regular);
            label8.Font = new Font("Constantia", 10, FontStyle.Regular);
            label9.Font = new Font("Constantia", 10, FontStyle.Regular);
            label12.Font = new Font("Constantia", 10, FontStyle.Regular);
            label14.Font = new Font("Constantia", 10, FontStyle.Regular);

            label6.Width = 460;
            label6.Height = 50;

            alertTimer = new System.Windows.Forms.Timer();
            alertTimer.Interval = 10000;
            alertTimer.Tick += AlertTimer_Tick;

            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.FromArgb(96, 176, 242);
            button1.Font = new Font("Constantia", 10, FontStyle.Regular);
            button1.AutoSize = false;
            button1.Width = Math.Max(200, TextRenderer.MeasureText(button1.Text, button1.Font).Width);
        }

        private void InitializeIcons() 
        {
            closeButton = new Label 
            {
                Text = "X",
                ForeColor = Color.FromArgb(126, 149, 148),
                Font = new Font("Arial", 10, FontStyle.Regular),
                Width = 30,
                Height = 30,
                Location = new Point(label6.Right + 160, label6.Top + 13),
                FlatStyle = FlatStyle.Flat
            };

            string successIconPath = Path.Combine(Application.StartupPath, "Images", "icons8-success-49.png");

            successIcon = new PictureBox 
            {
                BackColor = Color.FromArgb(190, 254, 198),
                Image = Image.FromFile(successIconPath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = 25,
                Height = 25,
                Location = new Point(label6.Left + 5, label6.Top + 13)
            };

            string errorIconPath = Path.Combine(Application.StartupPath, "Images", "icons8-error-48.png");

            errorIcon = new PictureBox 
            {
                BackColor = Color.FromArgb(254, 222, 224),
                Image = Image.FromFile(errorIconPath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = 25,
                Height = 25,
                Location = new Point(label6.Left + 5, label6.Top + 13)
            };

            tabPage1.Controls.Add(closeButton);
            tabPage1.Controls.Add(successIcon);
            tabPage1.Controls.Add(errorIcon);

            closeButton.BringToFront();
            successIcon.BringToFront();
            errorIcon.BringToFront();

            errorIcon.Visible = false;
            successIcon.Visible = false;
            closeButton.Visible = false;

            closeButton.Click += CloseButton_Click;
        }

        private void CloseButton_Click(object sender, EventArgs e) 
        {
            label6.Visible = false;
            closeButton.Visible = false;
            successIcon.Visible = false;
            errorIcon.Visible = false;
        }

        private async void ConnectMQTT() 
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("test.mosquitto.org", 1883)
                .Build();

            mqttClient.ConnectedAsync += async e => 
            {
                System.Diagnostics.Debug.WriteLine("Connected to MQTT broker.");

                await SubscribeToTopics();
            };

            mqttClient.DisconnectedAsync += async e => 
            {
                System.Diagnostics.Debug.WriteLine("Disconnected from MQTT broker.");
            };

            mqttClient.ApplicationMessageReceivedAsync += e => 
            {
                System.Diagnostics.Debug.WriteLine("Message received event triggered.");
                var message = e.ApplicationMessage;

                string topic = message.Topic;
                System.Diagnostics.Debug.WriteLine($"Received message on topic: {topic}");
                var payload = Encoding.UTF8.GetString(message.Payload);

                ProcessIncomingMessage(message.Topic, payload);
                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(options);
        }

        private async Task SubscribeToTopics() 
        {
            try 
            {
                await mqttClient.SubscribeAsync("rsa/mainpage/line_status");
                await mqttClient.SubscribeAsync("rsa/mainpage/m1_counter");
                await mqttClient.SubscribeAsync("rsa/mainpage/m2_counter");
                await mqttClient.SubscribeAsync("rsa/mainpage/m3_counter");
                await mqttClient.SubscribeAsync("rsa/mainpage/m4_counter");
                await mqttClient.SubscribeAsync("rsa/mainpage/m5_counter");
                await mqttClient.SubscribeAsync("rsa/prodpage/line_counter");
                await mqttClient.SubscribeAsync("rsa/prodpage/waste_counter");
                await mqttClient.SubscribeAsync("rsa/prodpage/target");

                System.Diagnostics.Debug.WriteLine("Subscribed to topics.");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Subscription failed: {ex.Message}");
            }
        }

        private void ProcessIncomingMessage(string topic, string payload) 
        {
            switch (topic) 
            {
                case "rsa/mainpage/line_status":
                    System.Diagnostics.Debug.WriteLine("Enters to production line");
                    UpdateLineStatus(payload);
                    break;
                case "rsa/mainpage/m1_counter":
                    UpdateCycleTime("M1", payload);
                    double.TryParse(payload, out double machineActiveTime);
                    M1_PieChart(machineActiveTime);
                    break;
                case "rsa/mainpage/m2_counter":
                    UpdateCycleTime("M2", payload);
                    double.TryParse(payload, out double machine2ActiveTime);
                    M2_PieChart(machine2ActiveTime);
                    break;
                case "rsa/mainpage/m3_counter":
                    UpdateCycleTime("M3", payload);
                    double.TryParse(payload, out double machine3ActiveTime);
                    M3_PieChart(machine3ActiveTime);
                    break;
                case "rsa/mainpage/m4_counter":
                    UpdateCycleTime("M4", payload);
                    double.TryParse(payload, out double machine4ActiveTime);
                    M4_PieChart(machine4ActiveTime);
                    break;
                case "rsa/mainpage/m5_counter":
                    UpdateCycleTime("M5", payload);
                    double.TryParse(payload, out double machine5ActiveTime);
                    M5_PieChart(machine5ActiveTime);
                    break;
                case "rsa/prodpage/line_counter":
                    UpdateProductionCounter(payload);
                    double.TryParse(payload, out lineCounter);
                    break;
                case "rsa/prodpage/waste_counter":
                    UpdateWasteCounter(payload);
                    double.TryParse(payload, out wasteCounter);
                    break;
                case "rsa/prodpage/target":
                    UpdateTargetCounter(payload);
                    double.TryParse(payload, out targetCounter);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Unhandled topic: {topic}");
                    break;
            }
        }

        private void UpdateLineStatus(string status) 
        {
            System.Diagnostics.Debug.WriteLine($"Received Status: {status}");

            if (label6.InvokeRequired) 
            {
                label6.Invoke(new Action<string>(UpdateLineStatus), status);
            } 
            else 
            {
                label6.Text = $"Production Status: {status}";
                successIcon.Visible = false;
                errorIcon.Visible = false;
                closeButton.Visible = false;

                label6.ForeColor = Color.Black;
                label6.BackColor = Color.Gray;
                label6.TextAlign = ContentAlignment.MiddleCenter;

                if (status == "1") 
                {
                    label6.Visible = true;
                    label6.Text = "Line in production";
                    label6.ForeColor = Color.Black;
                    label6.BackColor = Color.FromArgb(190, 254, 198);
                    label6.TextAlign = ContentAlignment.MiddleLeft;
                    successIcon.Visible = true;
                    label6.AutoSize = false;
                    label6.AutoEllipsis = true;
                    label6.Width = 460;
                    label6.Height = 50;
                    label6.Padding = new Padding(successIcon.Width + 10, 0, 0, 0);
                    closeButton.Visible = true;
                    closeButton.BackColor = Color.FromArgb(190, 254, 198);
                    closeButton.Cursor = Cursors.Hand;

                    StopDowntime();
                    StartUptime();
                    StartTimer();
                } 
                else if (status == "0") 
                {
                    label6.Visible = true;
                    label6.Text = "Line in stop";
                    label6.ForeColor = Color.Black;
                    label6.BackColor = Color.FromArgb(254, 222, 224);
                    label6.TextAlign = ContentAlignment.MiddleLeft;
                    errorIcon.Visible = true;
                    label6.AutoSize = false;
                    label6.AutoEllipsis = true;
                    label6.Width = 460;
                    label6.Height = 50;
                    label6.Padding = new Padding(errorIcon.Width + 10, 0, 0, 0);
                    closeButton.Visible = true;
                    closeButton.BackColor = Color.FromArgb(254, 222, 224);
                    closeButton.Cursor = Cursors.Hand;

                    StartDowntime();
                    StopUptime();
                    StartTimer();
                } 
                else 
                {
                    label6.Text = "Production Status: Unknown";
                    label6.ForeColor = Color.Black;
                    label6.BackColor = Color.Gray;
                    label6.TextAlign = ContentAlignment.MiddleCenter;

                    successIcon.Visible = false;
                    errorIcon.Visible = false;

                    StopTimer();
                }

                alertTimer.Start();
            }
        }

        private void AlertTimer_Tick(object sender, EventArgs e) 
        {
            successIcon.Visible = false;
            errorIcon.Visible = false;
            closeButton.Visible = false;
            label6.Visible = false;

            alertTimer.Stop();
        }

        private void StartTimer() 
        {
            lastStatusChangeTime = DateTime.Now;
        }

        private void StopTimer() 
        {
            if (lastStatusChangeTime != default(DateTime)) 
            {
                statusDuration = DateTime.Now - lastStatusChangeTime;  
                lastStatusChangeTime = default(DateTime); 
            }
        }

        private void StartDowntime() 
        {
            if (lastStopTime == default(DateTime)) 
            {
                lastStopTime = DateTime.Now;
            }
        }

        private void StopDowntime() 
        {
            if (lastStopTime != default(DateTime)) 
            {
                downtimeDuration = DateTime.Now - lastStopTime;
                totalDowntime += downtimeDuration;
                lastStopTime = default(DateTime);
            }
        }

        private void StartUptime() 
        {
            lineStartTime = DateTime.Now;
        }

        private void StopUptime() 
        {
            if (lineStartTime != default(DateTime)) 
            {
                TimeSpan currentUptime = DateTime.Now - lineStartTime;
                totalUptime += currentUptime;
                lineStartTime = default(DateTime);
            }
        }

        private void UpdateCycleTime(string machineId, string cycleTime) 
        {
            if (label1.InvokeRequired) 
            {
                label1.Invoke(new Action(() => UpdateCycleTime(machineId, cycleTime)));
            } 
            else 
            {
                if (machineLabels.TryGetValue(machineId, out Label label)) 
                {
                    label.Text = $"{machineId} cycle time: \n {cycleTime} seconds";
                }
            }
        }

        private void UpdateProductionCounter(string count) 
        {
            if (label7.InvokeRequired) 
            {
                label7.Invoke(new Action<string>(UpdateProductionCounter), count);
            } 
            else 
            {
                label7.Text = $"Production Counter: {count}";
            }
        }

        private void Form1_Load(object sender, EventArgs e) 
        {
            Image loadedImage = Image.FromFile(Path.Combine(Application.StartupPath, "Images", "img-removebg-preview.png"));

            pictureBox1.Image = loadedImage;
            pictureBox1.Width = 150;
            pictureBox1.Height = 150;
        }

        private void UpdateWasteCounter(string count) 
        {
            if (int.TryParse(count, out int wasteCount)) 
            {
                if (label8.InvokeRequired) 
                {
                    label8.Invoke(new Action<string>(UpdateWasteCounter), count);
                } 
                else 
                {
                    label8.Text = $"Waste Counter: {wasteCount}";
                }
            }
        }

        private void UpdateTargetCounter(string count) 
        {
            if (int.TryParse(count, out int targetCount)) 
            {
                if (label9.InvokeRequired) 
                {
                    label9.Invoke(new Action<string>(UpdateTargetCounter), count);
                } 
                else 
                {
                    label9.Text = $"Target Counter: {targetCount}";
                }
            }
        }

        private void UpdatePieChart(Panel targetPanel, double cycleTime, Point location, System.Windows.Media.Brush activeColor) 
        {
            if (this.InvokeRequired) 
            {
                this.Invoke(new Action(() => UpdatePieChart(targetPanel, cycleTime, location, activeColor)));
                return;
            }

            LiveCharts.WinForms.PieChart pieChart = new LiveCharts.WinForms.PieChart 
            {
                Size = new System.Drawing.Size(150, 150),
                Location = location
            };

            double totalTime = 60;
            double remainingTime = totalTime - cycleTime;

            SeriesCollection sers = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Machine Active",
                    Values = new ChartValues<double> { cycleTime },
                    DataLabels = true,
                    Fill = activeColor
                },
                new PieSeries
                {
                    Title = "Remaining Time",
                    Values = new ChartValues<double> { remainingTime },
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.Gray
                }
            };

            pieChart.Series = sers;

            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = pieChart;

            targetPanel.Controls.Clear();
            targetPanel.Controls.Add(pieChart);
        }

        private void M1_PieChart(double cycleTime) =>
            UpdatePieChart(panel1, cycleTime, new Point(10, 10), System.Windows.Media.Brushes.Green);

        private void M2_PieChart(double cycleTime) =>
            UpdatePieChart(panel2, cycleTime, new Point(170, 10), System.Windows.Media.Brushes.Orange);

        private void M3_PieChart(double cycleTime) =>
            UpdatePieChart(panel3, cycleTime, new Point(230, 10), System.Windows.Media.Brushes.Blue);

        private void M4_PieChart(double cycleTime) =>
            UpdatePieChart(panel4, cycleTime, new Point(390, 10), System.Windows.Media.Brushes.Yellow);

        private void M5_PieChart(double cycleTime) =>
            UpdatePieChart(panel5, cycleTime, new Point(450, 10), System.Windows.Media.Brushes.Red);

        private void button1_Click(object sender, EventArgs e) 
        {
            if (lineStartTime != default(DateTime)) 
            {
                StopUptime();
            }

            if (lastStopTime != default(DateTime)) 
            {
                StopDowntime();
            }

            double D = totalUptime.TotalMinutes / (totalUptime.TotalMinutes + totalDowntime.TotalMinutes);
            double E = lineCounter / targetCounter;
            double Q = (lineCounter - wasteCounter) / lineCounter;

            double OEE = D * E * Q * 100;

            System.Diagnostics.Debug.WriteLine($"Uptime: {totalUptime}, Downtime: {totalDowntime}, D: {D}, E: {E}, Q: {Q}, OEE: {OEE}");

            ShowOEEGauge(OEE);

            ShowMetricCard(panel8, IconType.Availability, "Availability", $"{D:F2}");
            ShowMetricCard(panel9, IconType.Performance, "Performance", $"{E:F2}");
            ShowMetricCard(panel10, IconType.Quality, "Quality", $"{Q:F2}");
        }

        private void ShowOEEGauge(double oee) 
        {
            if (this.InvokeRequired) 
            {
                this.Invoke(new Action(() => ShowOEEGauge(oee)));
                return;
            }

            LiveCharts.WinForms.SolidGauge gauge = new LiveCharts.WinForms.SolidGauge 
            {
                Width = 300,
                Height = 250,
                Margin = new Padding(10),
            };

            gauge.From = 0;
            gauge.To = 100;
            gauge.Value = oee; 
            gauge.LabelFormatter = value => $"{value:F2}%";

            this.panel7.Controls.Clear();
            this.panel7.Controls.Add(gauge);
        }

        private Image GetIconForType(IconType iconType) 
        {
            string imagePath = string.Empty;

            switch (iconType) 
            {
                case IconType.Availability:
                    imagePath = Path.Combine(Application.StartupPath, "Images", "icons8-bar-chart-100.png");
                    break;
                case IconType.Performance:
                    imagePath = Path.Combine(Application.StartupPath, "Images", "icons8-clock-100.png");
                    break;
                case IconType.Quality:
                    imagePath = Path.Combine(Application.StartupPath, "Images", "icons8-check-mark2-100.png");
                    break;
                default:
                    return null;
            }

            return Image.FromFile(imagePath);  
        }

        private void ShowMetricCard(Panel panel, IconType iconType, string metricName, string value) 
        {
            panel.Controls.Clear();  
            panel.BorderStyle = BorderStyle.FixedSingle; 
            panel.Width = 180;
            panel.Height = 250;

            PictureBox iconPictureBox = new PictureBox 
            {
                Image = GetIconForType(iconType),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Width = 80,
                Height = 120,
                Dock = DockStyle.Top,
                Margin = new Padding(10)
            };

            Label metricLabel = new Label 
            {
                Text = $"{metricName}: \n{value}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(226, 234, 243),
                Padding = new Padding(5)
            };

            panel.Controls.Add(iconPictureBox);
            panel.Controls.Add(metricLabel);
        }
    }
}
