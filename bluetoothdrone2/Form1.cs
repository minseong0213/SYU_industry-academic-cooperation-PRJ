using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Forms;
using bluetoothdrone2.Properties;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;


namespace DroneControlWinFormsApp
{
    public partial class Form1 : Form
    {
        // 필드 선언부
        private BluetoothClient _bluetoothClient;
        private List<BluetoothDeviceInfo> _availableDevices = new List<BluetoothDeviceInfo>();
        private BluetoothDeviceInfo? _connectedDevice = null; // null 허용
        private NetworkStream? _bluetoothStream = null; // null 허용

        private bool isDraggingLeftJoystick = false;
        private bool isDraggingRightJoystick = false;
        private Point leftJoystickStartPoint;
        private Point rightJoystickStartPoint;

        private Point leftJoystickInitialPosition;
        private Point rightJoystickInitialPosition;
        private Point leftJoystickOffset;
        private Point rightJoystickOffset;

        private System.Windows.Forms.Timer _connectionCheckTimer;


        // 로그 텍스트박스
        private TextBox logTextBox;

        public Form1()
        {
            InitializeComponent();

            // 조이스틱 핸들의 초기 위치 설정
            leftJoystickInitialPosition = leftJoystickHandlePictureBox.Location;
            rightJoystickInitialPosition = rightJoystickHandlePictureBox.Location;

            using (MemoryStream ms = new MemoryStream(Resources.left_joystick_background))
            {
                leftJoystickBackgroundPictureBox.Image = Image.FromStream(ms);
            }

            // 오른쪽 조이스틱 배경 설정
            using (MemoryStream ms = new MemoryStream(Resources.right_joystick_background))
            {
                rightJoystickBackgroundPictureBox.Image = Image.FromStream(ms);
            }

            // 왼쪽 조이스틱 핸들 설정
            using (MemoryStream ms = new MemoryStream(Resources.left_joystick_handle))
            {
                leftJoystickHandlePictureBox.Image = Image.FromStream(ms);
            }

            // 오른쪽 조이스틱 핸들 설정
            using (MemoryStream ms = new MemoryStream(Resources.right_joystick_handle))
            {
                rightJoystickHandlePictureBox.Image = Image.FromStream(ms);
            }
            _bluetoothClient = new BluetoothClient();

            // 연결 상태 확인을 위한 타이머 설정
            _connectionCheckTimer = new System.Windows.Forms.Timer();
            _connectionCheckTimer.Interval = 5000; // 5초마다 상태 체크
            _connectionCheckTimer.Tick += CheckConnectionStatus;
            _connectionCheckTimer.Start();

            // 로그 텍스트박스 설정
            logTextBox = new TextBox();
            logTextBox.Multiline = true;
            logTextBox.ReadOnly = true;
            logTextBox.ScrollBars = ScrollBars.Vertical;
            logTextBox.Dock = DockStyle.Bottom;
            logTextBox.Height = 100;
            this.Controls.Add(logTextBox);
        }

        private void CheckConnectionStatus(object sender, EventArgs e)
        {
            if (_bluetoothClient != null && !_bluetoothClient.Connected)
            {
                AddLog("블루투스 연결이 끊어졌습니다. 재연결 시도 중...");
                ReconnectToDevice();
            }
        }

        private void ReconnectToDevice()
        {
            try
            {
                if (_connectedDevice != null)
                {
                    _bluetoothClient = new BluetoothClient();
                    _bluetoothClient.Connect(_connectedDevice.DeviceAddress, BluetoothService.SerialPort);
                    _bluetoothStream = _bluetoothClient.GetStream();
                    MessageBox.Show("블루투스 재연결 성공");
                    AddLog("블루투스 재연결 성공");
                }
                else
                {
                    AddLog("재연결할 장치 정보가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                AddLog($"재연결 시도 실패: {ex.Message}");
            }
        }

        private void AddLog(string message)
        {
            logTextBox.AppendText($"{DateTime.Now}: {message}\r\n");
        }


        private void scanButton_Click(object sender, EventArgs e)
        {
            AddLog("장치 검색 시작");

            // 기존에 사용 중인 연결 해제
            if (_bluetoothStream != null)
            {
                _bluetoothStream.Close();
                _bluetoothStream = null;
            }

            if (_bluetoothClient != null && _bluetoothClient.Connected)
            {
                _bluetoothClient.Close();
                _bluetoothClient = null; // 클라이언트 객체 초기화
            }

            droneListBox.Items.Clear();
            _availableDevices = new List<BluetoothDeviceInfo>();

            // 블루투스 장치 재검색
            try
            {
                _bluetoothClient = new BluetoothClient(); // 새로운 BluetoothClient 객체 생성
                BluetoothDeviceInfo[] devices = _bluetoothClient.DiscoverDevices().ToArray();
                foreach (var device in devices)
                {
                    _availableDevices.Add(device);
                    droneListBox.Items.Add(device.DeviceName);
                }

                if (_availableDevices.Count == 0)
                {
                    MessageBox.Show("검색된 블루투스 장치가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"장치 검색 오류: {ex.Message}");
                AddLog($"장치 검색 오류: {ex.Message}");
            }
        }


        private void connectButton_Click(object sender, EventArgs e)
        {
            if (droneListBox.SelectedIndex == -1)
            {
                MessageBox.Show("연결할 장치를 선택하세요.");
                return;
            }

            _connectedDevice = _availableDevices[droneListBox.SelectedIndex];

            try
            {
                if (_connectedDevice != null) // _connectedDevice가 null이 아닌지 확인
                {
                    if (_bluetoothClient != null)
                    {
                        _bluetoothClient.Close();
                    }

                    _bluetoothClient = new BluetoothClient();
                    _bluetoothClient.Connect(_connectedDevice.DeviceAddress, BluetoothService.SerialPort);
                    _bluetoothStream = _bluetoothClient.GetStream();
                    MessageBox.Show("드론에 성공적으로 연결되었습니다.");
                    AddLog("드론에 성공적으로 연결되었습니다.");
                }
                else
                {
                    MessageBox.Show("선택된 장치가 없습니다.");
                    AddLog("연결할 장치를 선택하지 않음.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 오류: {ex.Message}");
                AddLog($"연결 오류: {ex.Message}");
            }
        }


        private void disconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_bluetoothStream != null)
                {
                    _bluetoothStream.Close();
                    _bluetoothStream = null; // 스트림 객체를 null로 설정
                }

                if (_bluetoothClient.Connected)
                {
                    _bluetoothClient.Close();
                }
                _bluetoothClient = null; // 클라이언트 객체를 null로 설정

                MessageBox.Show("드론과의 연결이 해제되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 해제 오류: {ex.Message}");
            }
        }


        // MSP 패킷 생성 함수
        private byte[] createMspCommand(byte command, byte[] data)
        {
            List<byte> packet = new List<byte>();
            byte checksum = 0;

            // 시작 헤더
            packet.Add((byte)'$');
            packet.Add((byte)'M');
            packet.Add((byte)'<');

            // 데이터 크기
            packet.Add((byte)data.Length);
            checksum ^= (byte)data.Length;

            // 명령 코드
            packet.Add(command);
            checksum ^= command;

            // 데이터 추가
            foreach (byte b in data)
            {
                packet.Add(b);
                checksum ^= b;
            }

            // 체크섬 추가
            packet.Add(checksum);

            return packet.ToArray();
        }

        private void sendDataButton_Click(object sender, EventArgs e)
        {
            if (_bluetoothStream != null && _bluetoothStream.CanWrite)
            {
                byte[] command = null;

                switch (commandComboBox.SelectedItem.ToString())
                {
                    case "ARM":
                        command = createMspCommand(151, new byte[] { }); // MSP_SET_ARM
                        break;
                    case "DISARM":
                        command = createMspCommand(152, new byte[] { }); // MSP_SET_DISARM
                        break;
                    case "TRIM_UP":
                        command = createMspCommand(153, new byte[] { }); // MSP_TRIM_UP
                        break;
                    case "TRIM_DOWN":
                        command = createMspCommand(154, new byte[] { }); // MSP_TRIM_DOWN
                        break;
                    default:
                        MessageBox.Show("올바른 명령을 선택하세요.");
                        return;
                }

                try
                {
                    _bluetoothStream.Write(command, 0, command.Length);
                    MessageBox.Show($"{commandComboBox.SelectedItem} 명령 전송 성공");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"명령 전송 오류: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("먼저 드론에 연결하세요.");
            }
        }

        // Form1_FormClosing 메서드에서 null 체크 추가
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_bluetoothStream != null)
            {
                _bluetoothStream.Close();
                _bluetoothStream = null;
            }

            if (_bluetoothClient != null && _bluetoothClient.Connected)
            {
                _bluetoothClient.Close();
            }
            _bluetoothClient = null;
        }

        //
        // 드론 제어부 
        //

        // 왼쪽 조이스틱 핸들 이벤트 핸들러
        private void leftJoystickHandlePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftJoystickOffset = new Point(e.X, e.Y);
                leftJoystickHandlePictureBox.Capture = true;
            }
        }

        private void leftJoystickHandlePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftJoystickHandlePictureBox.Capture)
            {
                int x = e.X + leftJoystickHandlePictureBox.Left - leftJoystickOffset.X;
                int y = e.Y + leftJoystickHandlePictureBox.Top - leftJoystickOffset.Y;

                // 조이스틱 배경 안에만 핸들이 있도록 제한
                x = Math.Max(leftJoystickBackgroundPictureBox.Left, Math.Min(x, leftJoystickBackgroundPictureBox.Right - leftJoystickHandlePictureBox.Width));
                y = Math.Max(leftJoystickBackgroundPictureBox.Top, Math.Min(y, leftJoystickBackgroundPictureBox.Bottom - leftJoystickHandlePictureBox.Height));

                leftJoystickHandlePictureBox.Location = new Point(x, y);

                // 드론 이동 로직 호출
                UpdateDroneMovement(x - leftJoystickInitialPosition.X, y - leftJoystickInitialPosition.Y);
            }
        }

        private void leftJoystickHandlePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            leftJoystickHandlePictureBox.Capture = false;

            // 조이스틱 핸들 초기 위치로 되돌리기
            leftJoystickHandlePictureBox.Location = leftJoystickInitialPosition;

            // 드론 정지
            StopDroneMovement();
        }



        // 오른쪽 조이스틱 핸들 이벤트 핸들러
        private void rightJoystickHandlePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rightJoystickOffset = new Point(e.X, e.Y);
                rightJoystickHandlePictureBox.Capture = true;
            }
        }


        private void rightJoystickHandlePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (rightJoystickHandlePictureBox.Capture)
            {
                int x = e.X + rightJoystickHandlePictureBox.Left - rightJoystickOffset.X;
                int y = e.Y + rightJoystickHandlePictureBox.Top - rightJoystickOffset.Y;

                // 움직임을 상하 또는 좌우로만 제한
                int deltaX = Math.Abs(x - rightJoystickInitialPosition.X);
                int deltaY = Math.Abs(y - rightJoystickInitialPosition.Y);

                // x 방향으로 움직임이 y 방향보다 크면 좌우로만 이동, 그렇지 않으면 상하로만 이동
                if (deltaX > deltaY)
                {
                    // 좌우로만 이동 가능하게 설정
                    y = rightJoystickInitialPosition.Y;
                }
                else
                {
                    // 상하로만 이동 가능하게 설정
                    x = rightJoystickInitialPosition.X;
                }

                // 조이스틱 배경 안에만 핸들이 있도록 제한
                x = Math.Max(rightJoystickBackgroundPictureBox.Left, Math.Min(x, rightJoystickBackgroundPictureBox.Right - rightJoystickHandlePictureBox.Width));
                y = Math.Max(rightJoystickBackgroundPictureBox.Top, Math.Min(y, rightJoystickBackgroundPictureBox.Bottom - rightJoystickHandlePictureBox.Height));

                rightJoystickHandlePictureBox.Location = new Point(x, y);

                // 드론 회전 및 고도 제어 로직 호출
                UpdateDroneRotationAndAltitude(x - rightJoystickInitialPosition.X, y - rightJoystickInitialPosition.Y);
            }
        }


        private void rightJoystickHandlePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            rightJoystickHandlePictureBox.Capture = false;

            //// 조이스틱 핸들 초기 위치로 되돌리기
            //rightJoystickHandlePictureBox.Location = rightJoystickInitialPosition;

            //// 드론 정지
            //StopDroneMovement();

        }



        private void ResetJoystickPosition(PictureBox handle, PictureBox background)
        {
            handle.Location = new Point(
                background.Location.X + background.Width / 2 - handle.Width / 2,
                background.Location.Y + background.Height / 2 - handle.Height / 2
            );
        }

        private void UpdateDroneMovement(int deltaX, int deltaY)
        {
            // deltaX, deltaY 값에 따라 드론의 속도 및 방향 제어
            // 예를 들어, 양수 deltaY는 전진, 음수 deltaY는 후진
            // 양수 deltaX는 오른쪽 기울기, 음수 deltaX는 왼쪽 기울기

            // 드론 제어 명령 보내기
            // 예: sendControlCommand("MOVE", deltaX, deltaY);
        }

        private void SendControlCommand(int rotationSpeed, int throttle)
        {
            int[] rcData = new int[8];
            rcData[0] = 1500; // Roll 중립
            rcData[1] = 1500; // Pitch 중립
            rcData[2] = Math.Clamp(throttle, 1000, 2000); // 스로틀
            rcData[3] = Math.Clamp(rotationSpeed, 1000, 2000); // Yaw
            rcData[4] = 1500; // Aux1 (기본값)
            rcData[5] = 1500; // Aux2 (기본값)
            rcData[6] = 1500; // Aux3 (기본값)
            rcData[7] = 1500; // Aux4 (기본값)

            List<byte> data = new List<byte>();
            foreach (int val in rcData)
            {
                data.Add((byte)(val >> 8)); // 상위 바이트
                data.Add((byte)(val & 0xFF)); // 하위 바이트
            }

            byte[] command = createMspCommand(150, data.ToArray()); // MSP_SET_RCDATA (150) 명령 사용
            SendToDrone(command); // 드론에 데이터 전송
            AddLog($"회전 속도: {rotationSpeed}, 스로틀: {throttle}로 드론 제어 명령 전송");
        }


        private void SendToDrone(byte[] command)
        {
            if (_bluetoothStream != null && _bluetoothStream.CanWrite)
            {
                try
                {
                    _bluetoothStream.Write(command, 0, command.Length);
                    AddLog($"명령 전송 성공: {BitConverter.ToString(command)}");
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show($"명령 전송 중 입출력 오류 발생: {ioEx.Message}");
                    AddLog($"명령 전송 중 입출력 오류 발생: {ioEx.Message}");
                    ReconnectToDevice(); // 연결 재시도
                }
                catch (SocketException sockEx)
                {
                    MessageBox.Show($"소켓 오류 발생: {sockEx.Message}");
                    AddLog($"소켓 오류 발생: {sockEx.Message}");
                    ReconnectToDevice(); // 연결 재시도
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"명령 전송 오류: {ex.Message}");
                    AddLog($"명령 전송 오류: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("먼저 드론에 연결하세요.");
                AddLog("드론에 연결되지 않음.");
            }
        }


        private void UpdateDroneRotationAndAltitude(int deltaX, int deltaY)
        {
            // deltaX에 따라 드론 회전, deltaY에 따라 드론 상승/하강 제어
            int rotationSpeed = 1500 + deltaX; // 기본값 1500에 좌우 이동량을 더하여 회전 속도 설정
            int throttle = 1500 - deltaY; // 기본값 1500에서 Y 축 이동량을 빼서 스로틀 설정

            // 스로틀 및 회전 속도를 1000에서 2000 사이로 제한
            rotationSpeed = Math.Clamp(rotationSpeed, 1000, 2000);
            throttle = Math.Clamp(throttle, 1000, 2000);

            // 드론 제어 명령 전송
            SendControlCommand(rotationSpeed, throttle);
        }

        private void StopDroneMovement()
        {
            // 드론 정지 명령
            SendControlCommand(1500, 1000); // 스로틀을 낮춰서 착륙 또는 정지
        }

    }
}
