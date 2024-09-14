using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using bluetoothdrone2.Properties; // 프로젝트 루트 네임스페이스에 맞게 수정하세요.

namespace DroneControlWinFormsApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            droneListLabel = new Label();
            droneListBox = new ListBox();
            scanButton = new Button();
            connectButton = new Button();
            commandComboBox = new ComboBox();
            sendDataButton = new Button();
            diconection = new Button();
            armAndTakeOffButton = new Button();
            landButton = new Button();
            leftJoystickBackgroundPictureBox = new PictureBox();
            rightJoystickBackgroundPictureBox = new PictureBox();
            leftJoystickHandlePictureBox = new PictureBox();
            rightJoystickHandlePictureBox = new PictureBox();
            //batteryVoltageLabel = new Label();
            ((ISupportInitialize)leftJoystickBackgroundPictureBox).BeginInit();
            ((ISupportInitialize)rightJoystickBackgroundPictureBox).BeginInit();
            ((ISupportInitialize)leftJoystickHandlePictureBox).BeginInit();
            ((ISupportInitialize)rightJoystickHandlePictureBox).BeginInit();
            SuspendLayout();
            // 
            // droneListLabel
            // 
            droneListLabel.AutoSize = true;
            droneListLabel.Location = new Point(12, 9);
            droneListLabel.Name = "droneListLabel";
            droneListLabel.Size = new Size(124, 20);
            droneListLabel.TabIndex = 0;
            droneListLabel.Text = "Available Drones";
            // 
            // droneListBox
            // 
            droneListBox.FormattingEnabled = true;
            droneListBox.Location = new Point(12, 32);
            droneListBox.Name = "droneListBox";
            droneListBox.Size = new Size(175, 324);
            droneListBox.TabIndex = 1;
            // 
            // scanButton
            // 
            scanButton.Location = new Point(207, 32);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(100, 40);
            scanButton.TabIndex = 2;
            scanButton.Text = "Scan";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // connectButton
            // 
            connectButton.Location = new Point(322, 32);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(100, 40);
            connectButton.TabIndex = 3;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // commandComboBox
            // 
            commandComboBox.FormattingEnabled = true;
            commandComboBox.Items.AddRange(new object[] { "ARM", "DISARM", "TRIM_UP", "TRIM_DOWN" });
            commandComboBox.Location = new Point(207, 78);
            commandComboBox.Name = "commandComboBox";
            commandComboBox.Size = new Size(100, 28);
            commandComboBox.TabIndex = 4;
            // 
            // sendDataButton
            // 
            sendDataButton.Location = new Point(207, 112);
            sendDataButton.Name = "sendDataButton";
            sendDataButton.Size = new Size(100, 40);
            sendDataButton.TabIndex = 5;
            sendDataButton.Text = "Send";
            sendDataButton.UseVisualStyleBackColor = true;
            sendDataButton.Click += sendDataButton_Click;
            // 
            // diconection
            // 
            diconection.Location = new Point(428, 32);
            diconection.Name = "diconection";
            diconection.Size = new Size(119, 40);
            diconection.TabIndex = 6;
            diconection.Text = "Disconnection";
            diconection.UseVisualStyleBackColor = true;
            diconection.Click += disconnectButton_Click;
            // 
            // armAndTakeOffButton
            // 
            armAndTakeOffButton.Location = new Point(322, 75);
            armAndTakeOffButton.Name = "armAndTakeOffButton";
            armAndTakeOffButton.Size = new Size(215, 40);
            armAndTakeOffButton.TabIndex = 7;
            armAndTakeOffButton.Text = "ARM and Take Off";
            armAndTakeOffButton.UseVisualStyleBackColor = true;
            armAndTakeOffButton.Click += armAndTakeOffButton_Click;
            // 
            // landButton
            // 
            landButton.Location = new Point(322, 117);
            landButton.Name = "landButton";
            landButton.Size = new Size(215, 40);
            landButton.TabIndex = 8;
            landButton.Text = "Land";
            landButton.UseVisualStyleBackColor = true;
            landButton.Click += landButton_Click;
            // 
            // leftJoystickBackgroundPictureBox
            // 
            leftJoystickBackgroundPictureBox.Location = new Point(283, 198);
            leftJoystickBackgroundPictureBox.Name = "leftJoystickBackgroundPictureBox";
            leftJoystickBackgroundPictureBox.Size = new Size(264, 251);
            leftJoystickBackgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftJoystickBackgroundPictureBox.TabIndex = 7;
            leftJoystickBackgroundPictureBox.TabStop = false;
            // 
            // rightJoystickBackgroundPictureBox
            // 
            rightJoystickBackgroundPictureBox.Location = new Point(613, 198);
            rightJoystickBackgroundPictureBox.Name = "rightJoystickBackgroundPictureBox";
            rightJoystickBackgroundPictureBox.Size = new Size(257, 251);
            rightJoystickBackgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightJoystickBackgroundPictureBox.TabIndex = 9;
            rightJoystickBackgroundPictureBox.TabStop = false;
            // 
            // leftJoystickHandlePictureBox
            // 
            leftJoystickHandlePictureBox.Location = new Point(389, 306);
            leftJoystickHandlePictureBox.Name = "leftJoystickHandlePictureBox";
            leftJoystickHandlePictureBox.Size = new Size(50, 50);
            leftJoystickHandlePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftJoystickHandlePictureBox.TabIndex = 8;
            leftJoystickHandlePictureBox.TabStop = false;
            leftJoystickHandlePictureBox.MouseDown += leftJoystickHandlePictureBox_MouseDown;
            leftJoystickHandlePictureBox.MouseMove += leftJoystickHandlePictureBox_MouseMove;
            leftJoystickHandlePictureBox.MouseUp += leftJoystickHandlePictureBox_MouseUp;
            // 
            // rightJoystickHandlePictureBox
            // 
            rightJoystickHandlePictureBox.Location = new Point(717, 306);
            rightJoystickHandlePictureBox.Name = "rightJoystickHandlePictureBox";
            rightJoystickHandlePictureBox.Size = new Size(50, 50);
            rightJoystickHandlePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightJoystickHandlePictureBox.TabIndex = 10;
            rightJoystickHandlePictureBox.TabStop = false;
            rightJoystickHandlePictureBox.MouseMove += rightJoystickHandlePictureBox_MouseMove;
            // 
            // batteryVoltageLabel
            // 
            //batteryVoltageLabel.Location = new Point(553, 32);
            //batteryVoltageLabel.Name = "batteryVoltageLabel";
            //batteryVoltageLabel.Size = new Size(120, 30);
            //batteryVoltageLabel.TabIndex = 11;
            //batteryVoltageLabel.Text = "Battery Voltage: ";
            // 
            // Form1
            // 
            ClientSize = new Size(1132, 578);
            Controls.Add(leftJoystickHandlePictureBox);
            Controls.Add(rightJoystickHandlePictureBox);
            Controls.Add(diconection);
            Controls.Add(sendDataButton);
            Controls.Add(commandComboBox);
            Controls.Add(connectButton);
            Controls.Add(scanButton);
            Controls.Add(droneListBox);
            Controls.Add(droneListLabel);
            Controls.Add(leftJoystickBackgroundPictureBox);
            Controls.Add(rightJoystickBackgroundPictureBox);
            Controls.Add(armAndTakeOffButton);
            Controls.Add(landButton);
            //Controls.Add(batteryVoltageLabel);
            Name = "Form1";
            Text = "Drone Control Interface";
            FormClosing += Form1_FormClosing;
            ((ISupportInitialize)leftJoystickBackgroundPictureBox).EndInit();
            ((ISupportInitialize)rightJoystickBackgroundPictureBox).EndInit();
            ((ISupportInitialize)leftJoystickHandlePictureBox).EndInit();
            ((ISupportInitialize)rightJoystickHandlePictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label droneListLabel;
        private System.Windows.Forms.ListBox droneListBox;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ComboBox commandComboBox;
        private System.Windows.Forms.Button sendDataButton;
        private System.Windows.Forms.Button diconection;
        private System.Windows.Forms.PictureBox leftJoystickBackgroundPictureBox;
        private System.Windows.Forms.PictureBox rightJoystickBackgroundPictureBox;
        private System.Windows.Forms.PictureBox leftJoystickHandlePictureBox; // 왼쪽 조이스틱 핸들
        private System.Windows.Forms.PictureBox rightJoystickHandlePictureBox; // 오른쪽 조이스틱 핸들
        private System.Windows.Forms.Button armAndTakeOffButton;
        private System.Windows.Forms.Button landButton;
        //private System.Windows.Forms.Label batteryVoltageLabel;
    }
}
