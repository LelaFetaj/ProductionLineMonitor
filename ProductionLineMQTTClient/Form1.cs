using Microsoft.VisualBasic.ApplicationServices;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductionLineMQTTClient {
    public partial class Form1 : Form {

        private IMqttClient mqttClient;
        private System.Windows.Forms.Label lblProductionStatus;
        private Dictionary<string, Label> machineLabels = new Dictionary<string, Label>();
        private int wasteCounter = 0;
        private int targetCounter = 0;

        public Form1() {
            InitializeComponent();

            machineLabels.Add("M1", label1);
            machineLabels.Add("M2", label2);
            machineLabels.Add("M3", label3);
            machineLabels.Add("M4", label4);
            machineLabels.Add("M5", label5);

            this.Load += new EventHandler(MainForm_Load);
        }

        private void MainForm_Load(object sender, EventArgs e) {
            // Initialize the labels with default values
            label1.Text = "M1 Cycle Time: Loading...";
            label6.Text = "Production Status: Unknown";
            label7.Text = "Production Counter: 0";
            label8.Text = "Waste Counter: 0";
            label11.Text = "OEE: 0%";
            label12.Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. \nLorem Ipsum has been the industry's standard dummy text ever since the 1500s, \nwhen an unknown printer took a galley of type and scrambled it to make a type specimen book.";

            ConnectMQTT();
        }

        private async void ConnectMQTT() {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("test.mosquitto.org", 1883)
                .Build();

            mqttClient.ConnectedAsync += async e => {
                System.Diagnostics.Debug.WriteLine("Connected to MQTT broker.");

                await SubscribeToTopics();
                await PublishTestMessage();
            };

            mqttClient.ApplicationMessageReceivedAsync += e => {
                System.Diagnostics.Debug.WriteLine("Message received event triggered.");
                var message = e.ApplicationMessage;

                string topic = message.Topic;
                System.Diagnostics.Debug.WriteLine($"Received message on topic: {topic}");
                var payload = Encoding.UTF8.GetString(message.Payload);

                ProcessIncomingMessage(message.Topic, payload);
                return Task.CompletedTask;
            };

            try {
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex) {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }

        private void UpdateLabel(string text) {
            if (label1.InvokeRequired) {
                label1.Invoke(new Action<string>(UpdateLabel), text);
            } else {
                label1.Text = text;
            }
        }

        private async Task SubscribeToTopics() {
            try {
                await mqttClient.SubscribeAsync("production/line/status");
                await mqttClient.SubscribeAsync("production/machine/cycleTime/M1");
                await mqttClient.SubscribeAsync("production/machine/cycleTime/M2");
                await mqttClient.SubscribeAsync("production/machine/cycleTime/M3");
                await mqttClient.SubscribeAsync("production/machine/cycleTime/M4");
                await mqttClient.SubscribeAsync("production/machine/cycleTime/M5");
                await mqttClient.SubscribeAsync("production/counter/production");
                await mqttClient.SubscribeAsync("production/counter/waste");
                await mqttClient.SubscribeAsync("production/counter/target");

                System.Diagnostics.Debug.WriteLine("Subscribed to topics.");
            }
            catch (Exception ex) {
                Console.WriteLine($"Subscription failed: {ex.Message}");
            }
        }

        private async Task PublishTestMessage() {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("production/line/status")
                .WithPayload("Production Active")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message);
            System.Diagnostics.Debug.WriteLine("Test message published.");

            var cycleTimeMessageM1 = new MqttApplicationMessageBuilder()
                .WithTopic("production/machine/cycleTime/M1")
                .WithPayload("30")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(cycleTimeMessageM1);
            System.Diagnostics.Debug.WriteLine("Test message for machine M1 cycle time published.");

            var cycleTimeMessageM2 = new MqttApplicationMessageBuilder()
               .WithTopic("production/machine/cycleTime/M2")
               .WithPayload("25")
               .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
               .WithRetainFlag()
               .Build();

            await mqttClient.PublishAsync(cycleTimeMessageM2);
            System.Diagnostics.Debug.WriteLine("Test message for machine M2 cycle time published.");

            var cycleTimeMessageM3 = new MqttApplicationMessageBuilder()
               .WithTopic("production/machine/cycleTime/M3")
               .WithPayload("20")
               .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
               .WithRetainFlag()
               .Build();

            await mqttClient.PublishAsync(cycleTimeMessageM3);
            System.Diagnostics.Debug.WriteLine("Test message for machine M3 cycle time published.");

            var cycleTimeMessageM4 = new MqttApplicationMessageBuilder()
               .WithTopic("production/machine/cycleTime/M4")
               .WithPayload("40")
               .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
               .WithRetainFlag()
               .Build();

            await mqttClient.PublishAsync(cycleTimeMessageM4);
            System.Diagnostics.Debug.WriteLine("Test message for machine M4 cycle time published.");

            var cycleTimeMessageM5 = new MqttApplicationMessageBuilder()
               .WithTopic("production/machine/cycleTime/M5")
               .WithPayload("45")
               .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
               .WithRetainFlag()
               .Build();

            await mqttClient.PublishAsync(cycleTimeMessageM5);
            System.Diagnostics.Debug.WriteLine("Test message for machine M5 cycle time published.");

            var productionCounterMessage = new MqttApplicationMessageBuilder()
                .WithTopic("production/counter/production")
                .WithPayload("50")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(productionCounterMessage);
            System.Diagnostics.Debug.WriteLine("Test message for production counter.");

            var wasteCounterMessage = new MqttApplicationMessageBuilder()
                .WithTopic("production/counter/waste")
                .WithPayload("5") 
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(wasteCounterMessage);
            System.Diagnostics.Debug.WriteLine("Test message for production waste counter.");

            var targetCounterMessage = new MqttApplicationMessageBuilder()
               .WithTopic("production/counter/target")
               .WithPayload("100")
               .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
               .WithRetainFlag()
               .Build();

            await mqttClient.PublishAsync(targetCounterMessage);
            System.Diagnostics.Debug.WriteLine("Test message for production target counter.");
        }

        private void ProcessIncomingMessage(string topic, string payload) {
            switch (topic) {
                case "production/line/status":
                    System.Diagnostics.Debug.WriteLine("Enters to production line");
                    UpdateLineStatus(payload);
                    break;
                case "production/machine/cycleTime/M1":
                    UpdateCycleTime("M1", payload);
                    break;
                case "production/machine/cycleTime/M2":
                    UpdateCycleTime("M2", payload);
                    break;
                case "production/machine/cycleTime/M3":
                    UpdateCycleTime("M3", payload);
                    break;
                case "production/machine/cycleTime/M4":
                    UpdateCycleTime("M4", payload);
                    break;
                case "production/machine/cycleTime/M5":
                    UpdateCycleTime("M5", payload);
                    break;
                case "production/counter/production":
                    UpdateProductionCounter(payload);
                    break;
                case "production/counter/waste":
                    UpdateWasteCounter(payload);
                    break;
                case "production/counter/target":
                    UpdateTargetCounter(payload);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Unhandled topic: {topic}");
                    break;
            }
        }

        private void UpdateLineStatus(string status) {
            System.Diagnostics.Debug.WriteLine($"Received Status: {status}");
            if (label6.InvokeRequired) {
                label6.Invoke(new Action<string>(UpdateLineStatus), status);
            } else {
                label6.Text = $"Line Status: {status}";
            }
        }

        private void UpdateCycleTime(string machineId, string cycleTime) {
            if (label1.InvokeRequired) {
                label1.Invoke(new Action(() => UpdateCycleTime(machineId, cycleTime)));
            } else {
                //label1.Text = $"{machineId} Cycle Time: {cycleTime}";
                if (machineLabels.TryGetValue(machineId, out Label label)) {
                    label.Text = $"{machineId} cycle time: \n {cycleTime} seconds";
                }
            }
        }

        private void UpdateProductionCounter(string count) {
            if (label7.InvokeRequired) {
                label7.Invoke(new Action<string>(UpdateProductionCounter), count);
            } else {
                label7.Text = $"Production Counter: {count}";
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            string imagePath = @"C:\Users\a.fetaj\source\repos\ProductionLineMQTTClient\ProductionLineMQTTClient\img.jpg";
            pictureBox1.Image = Image.FromFile(imagePath);
        }

        private void UpdateWasteCounter(string count) {
            if (int.TryParse(count, out int wasteCount)) {
                wasteCounter = wasteCount;
                if (label8.InvokeRequired) {
                    label8.Invoke(new Action<string>(UpdateWasteCounter), count);
                } else {
                    label8.Text = $"Waste Counter: {wasteCounter}";
                }
            }
        }

        private void UpdateTargetCounter(string count) {
            if (int.TryParse(count, out int targetCount)) {
                targetCounter = targetCount;
                if (label9.InvokeRequired) {
                    label9.Invoke(new Action<string>(UpdateTargetCounter), count);
                } else {
                    label9.Text = $"Target Counter: {targetCounter}";
                }
            }
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void label8_Click(object sender, EventArgs e) {

        }

        private void label6_Click(object sender, EventArgs e) {

        }

        private void label12_Click(object sender, EventArgs e) {

        }

        private void pictureBox1_Click(object sender, EventArgs e) {

        }
    }
}
