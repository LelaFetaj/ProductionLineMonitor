using LiveCharts;
using LiveCharts.Wpf;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Windows.Forms.Integration;

namespace ProductionLineMQTTClient 
{
    public partial class Form1 : Form 
    {
        private IMqttClient mqttClient;
        private System.Windows.Forms.Label lblProductionStatus;
        private Dictionary<string, Label> machineLabels = new Dictionary<string, Label>();

        private DateTime lastStopTime;
        private TimeSpan downtimeDuration;
        private DateTime lineStartTime;

        private TimeSpan totalUptime = TimeSpan.Zero;
        private TimeSpan totalDowntime = TimeSpan.Zero;

        private DateTime lastStatusChangeTime = default(DateTime); 
        private TimeSpan statusDuration = TimeSpan.Zero;

        private double lineCounter = 0;
        private double wasteCounter = 0;
        private double targetCounter = 0;

        private Label closeButton;
        private PictureBox successIcon;
        private PictureBox errorIcon;

        public Form1() 
        {
            InitializeComponent();

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
            label7.Text = "Line Production Counter: 0";
            label8.Text = "Waste Counter: 0";
            label9.Text = "Target: 0";
            label10.Text = "Downtime: 0";
            label13.Text = "Uptime: 0";
            label11.Text = "OEE: 0%";
            label12.Text = "Robot System Automation is a dynamic manufacturing company founded in 1984 with the goal of building \ninnovative, automated production systems for the footwear industry.\nWe have extensive experience in this highly competitive field. Experience, combined with the desire to \ncontinually improve our products, services and processes, has made Robot System Automation's state of \nthe art production facility a world leader both in the study of new technologies and in the development of \nnew concepts for the application of robots in the footwear industry.";

            label1.Font = new Font("Constantia", 10, FontStyle.Regular);
            label2.Font = new Font("Constantia", 10, FontStyle.Regular);
            label3.Font = new Font("Constantia", 10, FontStyle.Regular);
            label4.Font = new Font("Constantia", 10, FontStyle.Regular);
            label5.Font = new Font("Constantia", 10, FontStyle.Regular);
            label6.Font = new Font("Constantia", 10, FontStyle.Regular);
            label11.Font = new Font("Constantia", 10, FontStyle.Regular);
            label12.Font = new Font("Constantia", 10, FontStyle.Regular);

            label6.Width = 460;
            label6.Height = 50;
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

            successIcon = new PictureBox 
            {
                BackColor = Color.FromArgb(190, 254, 198),
                Image = Image.FromFile(@"C:\Users\a.fetaj\source\repos\ProductionLineMQTTClient\ProductionLineMQTTClient\icons8-success-49.png"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = 25,
                Height = 25,
                Location = new Point(label6.Left + 5, label6.Top + 13)
            };

            errorIcon = new PictureBox 
            {
                BackColor = Color.FromArgb(254, 222, 224),
                Image = Image.FromFile(@"C:\Users\a.fetaj\source\repos\ProductionLineMQTTClient\ProductionLineMQTTClient\icons8-error-48.png"),
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

                    // Stop downtime calculation when the line is in production
                    StopDowntime();
                    StartUptime();

                    StartTimer();
                } 
                else if (status == "0") 
                {
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

                    // Start downtime calculation when the line is stopped
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
            }
        }

        private void StartTimer() 
        {
            lastStatusChangeTime = DateTime.Now;  // Record the start time
        }

        private void StopTimer() 
        {
            if (lastStatusChangeTime != default(DateTime)) 
            {
                statusDuration = DateTime.Now - lastStatusChangeTime;  // Calculate the duration
                lastStatusChangeTime = default(DateTime);  // Reset the start time
            }
        }

        private void StartDowntime() 
        {
            if (lastStopTime == default(DateTime)) 
            {
                // Records the time when the line stops
                lastStopTime = DateTime.Now;
            }
        }

        private void StopDowntime() 
        {
            if (lastStopTime != default(DateTime)) 
            {
                // Calculate downtime duration
                downtimeDuration = DateTime.Now - lastStopTime;
                totalDowntime += downtimeDuration;
                label10.Text = $"Downtime: {totalDowntime.TotalMinutes:F2} minutes";
                // Reset the stop time
                lastStopTime = default(DateTime);
            }
        }

        private void StartUptime() 
        {
            // Track uptime start time
            lineStartTime = DateTime.Now;
        }

        private void StopUptime() 
        {
            if (lineStartTime != default(DateTime)) 
            {
                // Calculate uptime duration
                TimeSpan currentUptime = DateTime.Now - lineStartTime;

                totalUptime += currentUptime;

                label13.Text = $"Uptime: {totalUptime.TotalMinutes:F2} minutes";
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
                label7.Text = $"Line Production Counter: {count}";
            }
        }

        private void Form1_Load(object sender, EventArgs e) 
        {
            string imagePath = @"C:\Users\a.fetaj\source\repos\ProductionLineMQTTClient\ProductionLineMQTTClient\img-removebg-preview.png";
            pictureBox1.Image = Image.FromFile(imagePath);
            pictureBox1.Width = 100000;
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

        private void M1_PieChart(double cycleTime) 
        {
            UpdatePieChart(panel1, cycleTime, new Point(10, 10), System.Windows.Media.Brushes.Green);
        }

        private void M2_PieChart(double cycleTime) 
        {
            UpdatePieChart(panel2, cycleTime, new Point(170, 10), System.Windows.Media.Brushes.Orange);
        }

        private void M3_PieChart(double cycleTime) 
        {
            UpdatePieChart(panel3, cycleTime, new Point(230, 10), System.Windows.Media.Brushes.Blue);
        }

        private void M4_PieChart(double cycleTime) 
        {
            UpdatePieChart(panel4, cycleTime, new Point(390, 10), System.Windows.Media.Brushes.Yellow);
        }

        private void M5_PieChart(double cycleTime) 
        {
            UpdatePieChart(panel5, cycleTime, new Point(450, 10), System.Windows.Media.Brushes.Red);
        }

        private void button1_Click(object sender, EventArgs e) 
        {
            if (lineStartTime != default(DateTime)) 
            {
                StopUptime();
            }

            // Display the current total uptime in label13
            label13.Text = $"Uptime: {totalUptime.TotalMinutes:F2} minutes";

            double D = totalUptime.TotalMinutes / (totalUptime.TotalMinutes + downtimeDuration.TotalMinutes);
            double E = lineCounter / targetCounter; 
            double Q = (lineCounter - wasteCounter) / lineCounter; 

            // Calculate OEE
            double OEE = D * E * Q * 100;

            if (label11.InvokeRequired) 
            {
                label11.Invoke(new Action(() => 
                {
                    label11.Text = $"OEE: {OEE:F2}%";
                }));
            } 
            else 
            {
                label11.Text = $"OEE: {OEE:F2}%";
            }

            // Debugging for validation
            System.Diagnostics.Debug.WriteLine($"Uptime: {totalUptime}, Downtime: {downtimeDuration}, D: {D}, E: {E}, Q: {Q}, OEE: {OEE}");

            ShowOEEPieChart(D, E, Q);
        }

        private void ShowOEEPieChart(double availability, double performance, double quality) 
        {
            if (this.InvokeRequired) 
            {
                this.Invoke(new Action(() => ShowOEEPieChart(availability, performance, quality)));
                return;
            }

            LiveCharts.WinForms.PieChart pieChart = new LiveCharts.WinForms.PieChart 
            {
                Size = new System.Drawing.Size(250, 250),
                Location = new System.Drawing.Point(-5, -5) 
            };

            SeriesCollection sers = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Availability",
                    Values = new ChartValues<double> { availability * 100 },
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.Green
                },
                new PieSeries
                {
                    Title = "Performance",
                    Values = new ChartValues<double> { performance * 100 },
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.Blue
                },
                new PieSeries
                {
                    Title = "Quality",
                    Values = new ChartValues<double> { quality * 100 },
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.Orange
                }
            };

            pieChart.Series = sers;

            this.panel6.Controls.Clear();
            this.panel6.Controls.Add(pieChart);
        }

    }
}
