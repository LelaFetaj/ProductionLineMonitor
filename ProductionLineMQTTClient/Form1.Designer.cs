namespace ProductionLineMQTTClient {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label12 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            tabPage2 = new TabPage();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(3, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(795, 445);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label12);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(pictureBox1);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(787, 407);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Main Page";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(6, 146);
            label12.Name = "label12";
            label12.Size = new Size(135, 25);
            label12.TabIndex = 7;
            label12.Text = "paragraphLabel";
            label12.Click += label12_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(369, 33);
            label6.Name = "label6";
            label6.Size = new Size(180, 25);
            label6.TabIndex = 6;
            label6.Text = "Line Status: Unknown";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(621, 295);
            label5.Name = "label5";
            label5.Size = new Size(127, 25);
            label5.TabIndex = 5;
            label5.Text = "M5 Cycle Time";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(470, 295);
            label4.Name = "label4";
            label4.Size = new Size(127, 25);
            label4.TabIndex = 4;
            label4.Text = "M4 Cycle Time";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(326, 295);
            label3.Name = "label3";
            label3.Size = new Size(127, 25);
            label3.TabIndex = 3;
            label3.Text = "M3 Cycle Time";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(179, 295);
            label2.Name = "label2";
            label2.Size = new Size(127, 25);
            label2.TabIndex = 2;
            label2.Text = "M2 Cycle Time";
            label2.Click += label2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 295);
            label1.Name = "label1";
            label1.Size = new Size(127, 25);
            label1.TabIndex = 1;
            label1.Text = "M1 Cycle Time";
            label1.Click += label1_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(6, 6);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(300, 113);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label11);
            tabPage2.Controls.Add(label10);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(label8);
            tabPage2.Controls.Add(label7);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(787, 407);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Productivity Page";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(53, 258);
            label11.Name = "label11";
            label11.Size = new Size(63, 25);
            label11.TabIndex = 4;
            label11.Text = "lblOEE";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(53, 204);
            label10.Name = "label10";
            label10.Size = new Size(113, 25);
            label10.TabIndex = 3;
            label10.Text = "lblDowntime";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(53, 154);
            label9.Name = "label9";
            label9.Size = new Size(142, 25);
            label9.TabIndex = 2;
            label9.Text = "lblTargetCounter";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(53, 104);
            label8.Name = "label8";
            label8.Size = new Size(142, 25);
            label8.TabIndex = 1;
            label8.Text = "lblWasteCounter";
            label8.Click += label8_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(53, 48);
            label7.Name = "label7";
            label7.Size = new Size(186, 25);
            label7.TabIndex = 0;
            label7.Text = "Production Counter: 0";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label label1;
        private PictureBox pictureBox1;
        private Label label2;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label6;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label12;
    }
}
