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

        private int currentThrottle = 1000; // 현재 스로틀 값 (초기값은 1000)
        private int targetThrottle = 1500;  // 목표 스로틀 값
        private bool isConnected = false; // 연결 상태를 저장하는 변수
        private bool isThrottleActive = false; // 스로틀 값 전송 활성화 여부
        // 로그 텍스트박스
        private TextBox logTextBox;

        public Form1()
        {
            InitializeComponent();

            // 폼에서 키 입력을 받을 수 있도록 설정
            this.KeyPreview = true;

            // KeyDown 이벤트 핸들러 추가
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

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

            // 타이머 설정
            _connectionCheckTimer = new System.Windows.Forms.Timer();
            _connectionCheckTimer.Interval = 100; // 100ms 간격으로 설정 (조정 가능)
            _connectionCheckTimer.Tick += _connectionCheckTimer_Tick; // 이벤트 핸들러 추가
            _connectionCheckTimer.Start();

            // 초기화 및 연결 상태 확인
            _bluetoothClient = new BluetoothClient();
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

        private void _connectionCheckTimer_Tick(object sender, EventArgs e)
        {
            // ARM 명령이 전송된 경우에만 스로틀 값을 업데이트
            if (isThrottleActive)
            {
                SendControlCommand(1500, targetThrottle); // 스로틀 값을 유지하면서 지속적으로 전송
            }

            // 기존의 연결 상태 체크
            CheckConnectionStatus(sender, e);

            //// 배터리 상태를 주기적으로 요청 및 수신
            //ReceiveBatteryStatus();
        }



        private void CheckConnectionStatus(object sender, EventArgs e)
        {
            if (_bluetoothClient != null && !_bluetoothClient.Connected)
            {
                if (isConnected) // 연결이 끊어졌을 때만 메시지 표시
                {
                    isConnected = false; // 연결 상태 업데이트
                    AddLog("블루투스 연결이 끊어졌습니다. 재연결 시도 중...");
                    MessageBox.Show("블루투스 연결이 끊어졌습니다.");
                }
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

                    if (!isConnected) // 연결 상태가 변할 때만 메시지 표시
                    {
                        isConnected = true; // 연결 상태 업데이트
                        MessageBox.Show("드론에 성공적으로 연결되었습니다.");
                        AddLog("드론에 성공적으로 연결되었습니다.");
                    }
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

        // 블루투스 데이터 응답
        //private void ReceiveBluetoothData()
        //{
        //    if (_bluetoothStream != null && _bluetoothStream.CanRead)
        //    {
        //        try
        //        {
        //            // MSP 패킷은 6~10 바이트 정도로 예상 (패킷 크기 설정)
        //            byte[] buffer = new byte[10];
        //            int bytesRead = _bluetoothStream.Read(buffer, 0, buffer.Length);

        //            if (bytesRead > 0)
        //            {
        //                // 수신한 데이터를 MSP 패킷으로 처리
        //                ProcessMspResponse(buffer);
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            AddLog($"블루투스 데이터 수신 오류: {ex.Message}");
        //        }
        //    }
        //}

        // MSP 응답 함수 
        //private void ProcessMspResponse(byte[] response)
        //{
        //    if (response[0] == '$' && response[1] == 'M' && response[2] == '>')
        //    {
        //        byte dataSize = response[3];   // 데이터 크기
        //        byte command = response[4];    // 명령 코드

        //        //if (command == 110 && dataSize == 2)  // MSP_ANALOG 응답
        //        //{
        //        //    // 응답 데이터에서 배터리 전압 및 mAh 추출
        //        //    int voltage = response[5];  // 배터리 전압 (단위: 0.1V)

        //        //    // 배터리 상태 UI 업데이트
        //        //    UpdateBatteryStatus(voltage);
        //        //}
        //    }
        //}


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
            // ARM 명령을 보내는 로직
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
                        isThrottleActive = false; // DISARM일 때 스로틀 전송 중지
                        break;
                    default:
                        MessageBox.Show("올바른 명령을 선택하세요.");
                        return;
                }

                try
                {
                    _bluetoothStream.Write(command, 0, command.Length);
                    MessageBox.Show($"{commandComboBox.SelectedItem} 명령 전송 성공");

                    // ARM 명령이 전송되었을 때 스로틀 전송 활성화
                    if (commandComboBox.SelectedItem.ToString() == "ARM")
                    {
                        isThrottleActive = true; // ARM 상태일 때 스로틀 전송 활성화
                    }
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

                //// 드론 이동 로직 호출
                //UpdateDroneMovement(x - leftJoystickInitialPosition.X, y - leftJoystickInitialPosition.Y);
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



        //// 오른쪽 조이스틱 핸들 이벤트 핸들러
        //private void rightJoystickHandlePictureBox_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        rightJoystickOffset = new Point(e.X, e.Y);
        //        rightJoystickHandlePictureBox.Capture = true;
        //    }
        //}
        private void rightJoystickHandlePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (rightJoystickHandlePictureBox.Capture)
            {
                int x = e.X + rightJoystickHandlePictureBox.Left - rightJoystickOffset.X;
                int y = e.Y + rightJoystickHandlePictureBox.Top - rightJoystickOffset.Y;

                int maxX = rightJoystickBackgroundPictureBox.Right - rightJoystickHandlePictureBox.Width;
                int maxY = rightJoystickBackgroundPictureBox.Bottom - rightJoystickHandlePictureBox.Height;
                int minX = rightJoystickBackgroundPictureBox.Left;
                int minY = rightJoystickBackgroundPictureBox.Top;

                x = Math.Clamp(x, minX, maxX);
                y = Math.Clamp(y, minY, maxY);

                rightJoystickHandlePictureBox.Location = new Point(x, y);

                // 스로틀 값을 조정하는 로직
                ControlThrottleForTakeoff(y - rightJoystickInitialPosition.Y);
                SendControlCommand(1500, targetThrottle); // 회전 속도는 1500으로 고정
            }
        }

        //private void ControlThrottleForTakeoff(int deltaY)
        //{
        //    int maxThrottle = 2000; // 스로틀 최대값
        //    int minThrottle = 1000; // 스로틀 최소값
        //    // 조이스틱 이동량에 따라 목표 스로틀 값을 설정
        //    targetThrottle = 1500 - deltaY;

        //    // 스로틀 값을 안전한 범위로 제한
        //    targetThrottle = Math.Clamp(targetThrottle, minThrottle, maxThrottle);

        //    // 로그 출력
        //    AddLog($"목표 스로틀 값 설정: {targetThrottle}");

        //    // 스로틀 값을 설정한 후에도 계속 유지하고 드론에 전송합니다.
        //    SendControlCommand(1500, targetThrottle);



        //}

        private void ControlThrottleForTakeoff(int deltaY)
        {
            int minThrottle = 1000;   // 스로틀 최소값
            int midThrottle = 1500;   // 스로틀 중앙값
            int maxThrottle = 2000;   // 스로틀 최대값

            // 조이스틱 이동 범위 계산
            int maxY = rightJoystickBackgroundPictureBox.Bottom - rightJoystickHandlePictureBox.Height;  // 조이스틱의 Y축 최대 위치
            int minY = rightJoystickBackgroundPictureBox.Top;  // 조이스틱의 Y축 최소 위치
            int midY = (minY + maxY) / 2;  // 조이스틱의 Y축 중앙 위치

            int joystickY = rightJoystickHandlePictureBox.Top;  // 현재 조이스틱의 Y 위치

            if (joystickY < midY)
            {
                // 조이스틱이 중앙 위쪽에 있을 때: 1500에서 2000으로 증가
                targetThrottle = midThrottle + ((midY - joystickY) * (maxThrottle - midThrottle) / (midY - minY));
            }
            else
            {
                // 조이스틱이 중앙 아래쪽에 있을 때: 1500에서 1000으로 감소
                targetThrottle = midThrottle - ((joystickY - midY) * (midThrottle - minThrottle) / (maxY - midY));
            }

            // 스로틀 값을 제한 (1000에서 2000 사이)
            targetThrottle = Math.Clamp(targetThrottle, minThrottle, maxThrottle);

            // 로그 출력 (선택 사항)
            AddLog($"목표 스로틀 값 설정: {targetThrottle}");

            // 스로틀 값을 드론에 전송
            SendControlCommand(1500, targetThrottle);  // 회전 속도는 1500으로 고정
        }




        private void UpdateThrottle()
        {
            //// 목표 스로틀 값까지 점진적으로 증가
            //if (currentThrottle < targetThrottle)
            //{
            //    currentThrottle += 10; // 10씩 증가 (조정 가능)
            //    if (currentThrottle > targetThrottle)
            //    {
            //        currentThrottle = targetThrottle;
            //    }
            //}
            //else if (currentThrottle > targetThrottle)
            //{
            //    currentThrottle -= 10; // 스로틀 감소 시에도 점진적으로
            //    if (currentThrottle < targetThrottle)
            //    {
            //        currentThrottle = targetThrottle;
            //    }
            //}

            // 현재 스로틀 값으로 드론 제어 명령 전송
            SendControlCommand(1500, currentThrottle); // Yaw 값은 중립으로 고정 (1500)
        }




        //private void rightJoystickHandlePictureBox_MouseUp(object sender, MouseEventArgs e)
        //{
        //    rightJoystickHandlePictureBox.Capture = false;

        //    //// 조이스틱 핸들 초기 위치로 되돌리기
        //    //rightJoystickHandlePictureBox.Location = rightJoystickInitialPosition;

        //    //// 드론 정지
        //    //StopDroneMovement();

        //}



        private void ResetJoystickPosition(PictureBox handle, PictureBox background)
        {
            handle.Location = new Point(
                background.Location.X + background.Width / 2 - handle.Width / 2,
                background.Location.Y + background.Height / 2 - handle.Height / 2
            );
        }

        //private void UpdateDroneMovement(int deltaX, int deltaY)
        //{
        //    // deltaX, deltaY 값에 따라 드론의 속도 및 방향 제어
        //    // 예를 들어, 양수 deltaY는 전진, 음수 deltaY는 후진
        //    // 양수 deltaX는 오른쪽 기울기, 음수 deltaX는 왼쪽 기울기

        //    // 드론 제어 명령 보내기
        //    // 예: sendControlCommand("MOVE", deltaX, deltaY);
        //}

        private void SendControlCommand(int rotationSpeed, int throttle)
        {
            int[] rcData = new int[8];
            rcData[0] = 1500; // Roll 중립
            rcData[1] = 1500; // Pitch 중립
            rcData[2] = Math.Clamp(throttle, 1000, 2000); // 스로틀
            rcData[3] = 1500;
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
            if (_bluetoothClient != null && _bluetoothClient.Connected && _bluetoothStream != null && _bluetoothStream.CanWrite)
            {
                try
                {
                    _bluetoothStream.Write(command, 0, command.Length);
                    AddLog($"명령 전송 성공: {BitConverter.ToString(command)}");
                    // 명령을 보낸 후 잠시 대기
                    //System.Threading.Thread.Sleep(50); // 100ms 대기 (필요에 따라 조정)
                }
                catch (IOException ioEx)
                {
                    AddLog($"명령 전송 중 입출력 오류 발생: {ioEx.Message}");
                    ReconnectToDevice(); // 연결 재시도
                }
                catch (SocketException sockEx)
                {
                    AddLog($"소켓 오류 발생: {sockEx.Message}");
                    ReconnectToDevice(); // 연결 재시도
                }
                catch (Exception ex)
                {
                    AddLog($"명령 전송 오류: {ex.Message}");
                }
            }
            else
            {
                // 연결이 끊겼거나 연결이 안된 상태에서 반복적인 메시지를 방지하기 위한 개선
                if (!_bluetoothClient.Connected)
                {
                    AddLog("드론과의 연결이 끊어졌습니다. 다시 연결하세요.");
                }
                else if (_bluetoothStream == null || !_bluetoothStream.CanWrite)
                {
                    AddLog("블루투스 스트림이 유효하지 않거나 쓰기 불가능합니다.");
                }
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

        private void armAndTakeOffButton_Click(object sender, EventArgs e)
        {
            // ARM 명령을 보내고 스로틀 제어 활성화
            if (_bluetoothStream != null && _bluetoothStream.CanWrite)
            {
                byte[] command = createMspCommand(151, new byte[] { }); // MSP_SET_ARM
                SendToDrone(command);
                isThrottleActive = true; // 스로틀 활성화
                AddLog("드론이 ARM 상태로 전환되었습니다. 이륙을 준비합니다.");

                // 스로틀을 점진적으로 증가시켜 이륙
                IncreaseThrottleGradually();
            }
            else
            {
                MessageBox.Show("먼저 드론에 연결하세요.");
            }
        }

        // 스로틀 값을 서서히 증가시키는 함수
        private void IncreaseThrottleGradually()
        {
            if (isThrottleActive)
            {
                // 스로틀 값을 1000에서 목표치(예: 1800)까지 점진적으로 증가시킴
                targetThrottle = 1000; // 스로틀 초기값

                while (targetThrottle < 1800) // 1800까지 스로틀 증가
                {
                    // 드론에 명령 전송 (회전 속도는 1500 고정, 스로틀만 증가)
                    SendControlCommand(1500, targetThrottle);

                    targetThrottle += 50; // 50씩 증가
                    System.Threading.Thread.Sleep(500); // 500ms 대기 (서서히 증가하는 효과)

                    AddLog($"현재 스로틀 값: {targetThrottle}");
                }

                AddLog("드론이 이륙 중입니다.");
            }
        }

        private void landButton_Click(object sender, EventArgs e)
        {
            if (isThrottleActive)
            {
                targetThrottle = 1800; // 현재 스로틀 값을 1800에서 시작
                while (targetThrottle > 1000)
                {
                    SendControlCommand(1500, targetThrottle); // 회전 속도는 중립(1500)으로 설정
                    targetThrottle -= 50; // 50씩 감소
                    System.Threading.Thread.Sleep(500); // 500ms 대기
                }

                // 스로틀을 최소로 설정한 후 Disarm
                byte[] command = createMspCommand(152, new byte[] { }); // MSP_SET_DISARM
                SendToDrone(command);
                isThrottleActive = false; // 스로틀 비활성화
                AddLog("드론이 착륙 후 Disarm 상태로 전환되었습니다.");
            }
        }

        // 배터리 상태 체크

        //private void ReceiveBatteryStatus()
        //{
        //    if (_bluetoothStream != null && _bluetoothStream.CanRead)
        //    {
        //        try
        //        {
        //            // 데이터 수신 (MSP_ANALOG 명령어로 예상)
        //            byte[] buffer = new byte[7]; // MSP_ANALOG 명령은 7바이트 정도의 패킷을 수신한다고 가정
        //            int bytesRead = _bluetoothStream.Read(buffer, 0, buffer.Length);

        //            if (bytesRead > 0 && buffer[0] == '$' && buffer[1] == 'M' && buffer[2] == '>')
        //            {
        //                // 패킷 확인 후 배터리 상태 값 추출
        //                byte voltage = buffer[5]; // 전압 정보 (배터리 전압이 0.1V 단위로 전송됨, 예: 126 -> 12.6V)

        //                float batteryVoltage = voltage / 10.0f; // 0.1V 단위로 전송되므로 10으로 나눔
        //                AddLog($"Battery Voltage: {batteryVoltage}V");

        //                // UI에 업데이트하거나 추가 처리
        //                UpdateBatteryStatus(batteryVoltage);
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            AddLog($"배터리 상태 수신 중 오류 발생: {ex.Message}");
        //        }
        //        catch (Exception ex)
        //        {
        //            AddLog($"알 수 없는 오류 발생: {ex.Message}");
        //        }
        //    }
        //}

        //// 배터리 상태 업데이트 메서드 (UI 업데이트 가능)
        //private void UpdateBatteryStatus(float voltage)
        //{
        //    float batteryVoltage = voltage / 10.0f; // 0.1V 단위로 전송됨

        //    // UI 요소 업데이트 (Label 등)
        //    batteryVoltageLabel.Text = $"Voltage: {batteryVoltage}V";

        //    // 로그 업데이트
        //    AddLog($"Battery Voltage: {batteryVoltage}V");
        //}

        private void SendTrimCommand(byte trimCommand)
        {
            // MSP 명령어 생성 (데이터는 비어 있음)
            byte[] command = createMspCommand(trimCommand, new byte[] { });

            // 명령을 드론으로 전송
            SendToDrone(command);

            // 로그 출력
            AddLog($"Trim Command Sent: {trimCommand}");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: // 드론 앞으로 이동
                    MoveDroneForward();
                    break;
                case Keys.S: // 드론 뒤로 이동
                    MoveDroneBackward();
                    break;
                case Keys.A: // 드론 왼쪽 이동
                    MoveDroneLeft();
                    break;
                case Keys.D: // 드론 오른쪽 이동
                    MoveDroneRight();
                    break;

                // 트림 조정
                case Keys.Up: // Pitch 트림 증가
                    SendTrimCommand(153); // MSP_TRIM_UP
                    AddLog("Trim Up (Pitch)");
                    break;
                case Keys.Down: // Pitch 트림 감소
                    SendTrimCommand(154); // MSP_TRIM_DOWN
                    AddLog("Trim Down (Pitch)");
                    break;
                case Keys.Left: // Roll 트림 감소
                    SendTrimCommand(155); // MSP_TRIM_LEFT
                    AddLog("Trim Left (Roll)");
                    break;
                case Keys.Right: // Roll 트림 증가
                    SendTrimCommand(156); // MSP_TRIM_RIGHT
                    AddLog("Trim Right (Roll)");
                    break;

                default:
                    break;
            }
        }

        private void MoveDroneForward()
        {
            int pitchCommand = 1600;  // 예시로 pitch 값을 증가시켜 앞으로 이동
            int throttle = targetThrottle; // 현재 스로틀 유지
            SendControlCommand(pitchCommand, throttle);
            AddLog("Move Forward");
        }

        private void MoveDroneBackward()
        {
            int pitchCommand = 1400;  // 예시로 pitch 값을 감소시켜 뒤로 이동
            int throttle = targetThrottle; // 현재 스로틀 유지
            SendControlCommand(pitchCommand, throttle);
            AddLog("Move Backward");
        }

        private void MoveDroneLeft()
        {
            int rollCommand = 1400;  // 예시로 roll 값을 감소시켜 왼쪽으로 이동
            int throttle = targetThrottle; // 현재 스로틀 유지
            SendControlCommand(rollCommand, throttle);
            AddLog("Move Left");
        }

        private void MoveDroneRight()
        {
            int rollCommand = 1600;  // 예시로 roll 값을 증가시켜 오른쪽으로 이동
            int throttle = targetThrottle; // 현재 스로틀 유지
            SendControlCommand(rollCommand, throttle);
            AddLog("Move Right");
        }



    }
}
