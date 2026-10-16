using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace FormApp
{
    public partial class Form1 : Form
    {
        // 宣告通訊客戶端
        private FX5UClient _plcClient;

        // 宣告取消控制權杖，用來控制背景迴圈的停止
        private CancellationTokenSource? _cts;

        // 標記目前是否正在監控中
        private bool _isPolling = false;
        public Form1()
        {
            InitializeComponent();
            // 初始化 PLC 客戶端 (請換成您的 PLC IP 與 Port)
            _plcClient = new FX5UClient("192.168.3.250", 5002, timeoutMs: 1500);

            // 初始按鈕狀態
            btnStartPoll.Enabled = true;
            btnStopPoll.Enabled = false;
            btn_write_Click.Enabled = true;
        }

        private void btn_Stop_Poll(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel(); // 發出取消訊號，通知背景迴圈中斷
                _cts.Dispose();
                _cts = null;
            }

            _isPolling = false;
            btnStartPoll.Enabled = true;
            btnStopPoll.Enabled = false;
            txt_lblStatus.Text = "監控已停止";
            txt_lblStatus.ForeColor = Color.Gray;
        }

        private void btn_Start_Poll(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            _isPolling = true;

            btnStartPoll.Enabled = false;
            btnStopPoll.Enabled = true;
            txt_lblStatus.Text = "監控啟動中...";
            txt_lblStatus.ForeColor = Color.Orange;

            // 💡 核心：啟動一個完全獨立的背景工作進行定時迴圈
            Task.Run(() => DoPlcPollingLoopAsync(_cts.Token));
        }
        /// <summary>
        /// 【背景工作】核心定時讀取迴圈
        /// </summary>
        private async Task DoPlcPollingLoopAsync(CancellationToken token)
        {
            // 設定定時器（例如 500 毫秒刷新一次）
            // 如果您的 .NET 版本不支援 PeriodicTimer，可以使用 Task.Delay(500, token) 替代
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            while (_isPolling && !token.IsCancellationRequested)
            {
                try
                {
                    // 1. 執行非同步批量讀取 (從 D100 開始讀取連續 5 個 Word)
                    short[] plcValues = await _plcClient.ReadAsync(100, 5);

                    // 2. 💡 使用 InvokeAsync (或安全上下文) 將資料刷回 UI
                    // 因為我們在前文提及 await 會自動回原執行緒，但 Task.Run 的最上層需要明確指派回 UI
                    this.BeginInvoke(new Action(() =>
                    {
                        // 更新數值到視窗畫面上
                        txtD100.Text = plcValues.ToString();
                        txtD101.Text = plcValues.ToString();
                        txtD102.Text = plcValues.ToString();
                        txtD103.Text = plcValues.ToString();
                        txtD104.Text = plcValues.ToString();

                        txt_lblStatus.Text = $"通訊正常 ({DateTime.Now:HH:mm:ss})";
                        txt_lblStatus.ForeColor = Color.Green;
                    }));
                }
                catch (OperationCanceledException)
                {
                    // 使用者按了停止，正常登出迴圈
                    break;
                }
                catch (Exception ex)
                {
                    // 捕捉斷線、逾時或 PLC 報錯
                    this.BeginInvoke(new Action(() =>
                    {
                        txt_lblStatus.Text = $"通訊異常: {ex.Message}";
                        txt_lblStatus.ForeColor = Color.Red;

                        // 斷線時可選擇將數值欄位清空或變灰
                        txtD100.Text = "---";
                    }));
                }

                try
                {
                    // 3. 等待下一個 500 毫秒週期
                    await timer.WaitForNextTickAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /// <summary>
        /// 當視窗關閉時，務必強制釋放背景工作，防止軟體變成殭屍程式
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isPolling = false;
            _cts?.Cancel();
            base.OnFormClosing(e);
        }

        private async void btn_write_click(object sender, EventArgs e)
        {
            // 1. 驗證並解析 UI 輸入的數值（做基本防呆）
            if (!short.TryParse(txtD100.Text, out short val1) ||
                !short.TryParse(txtD101.Text, out short val2))
            {
                MessageBox.Show("請輸入正確的整數數值（範圍: -32768 ~ 32767）", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 停用按鈕，防止使用者在通訊期間連續瘋狂點擊
            btn_write_Click.Enabled = false;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 準備寫入資料，正在排隊通訊...\r\n");

            try
            {
                // 3. 準備要隨機寫入的多個不連續目標
                var writeList = new List<FX5UClient.WriteTarget>()
                {
                    new FX5UClient.WriteTarget(100, val1), // 將第一個值寫入 D100
                    new FX5UClient.WriteTarget(150, val2)  // 將第二個值寫入 D150
                };

                // 4. 💡 呼叫非同步寫入（底層 SemaphoreSlim 會自動與背景定時讀取排隊，不撞框）
                // 在等待排隊或 PLC 通訊時，UI 畫面依然完全流暢、不卡死
                await _plcClient.RandomWriteAsync(writeList);

                // 5. 💡 寫入成功後，自動返回 UI 執行緒，直接安全更新畫面
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 【成功】D100={val1}, D150={val2} 寫入完成！\r\n");
            }
            catch (TimeoutException)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 【逾時】PLC 未能在規定時間內回應寫入。\r\n");
            }
            catch (Exception ex)
            {
                // 捕捉其他突發異常（例如排隊排到一半網路斷開、PLC 報錯等）
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 【寫入失敗】{ex.Message}\r\n");
            }
            finally
            {
                // 6. 無論成功或失敗，最後務必恢復按鈕可用狀態
                btn_write_Click.Enabled = true;
            }
        }
    }
    public class FX5UClient
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly int _timeoutMs;
        // 💡 核心：定義非同步排隊鎖（初始 1 個通行證，最大 1 個通行證）
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public FX5UClient(string ip, int port, int timeoutMs = 2000)
        {
            _ip = ip;
            _port = port;
            _timeoutMs = timeoutMs;
        }

        /// <summary>
        /// 結構：隨機寫入的標的
        /// </summary>
        public struct WriteTarget
        {
            public int Address { get; set; }
            public byte DeviceCode { get; set; } // D暫存器固定為 0xA8
            public short Value { get; set; }

            public WriteTarget(int address, short value)
            {
                Address = address;
                DeviceCode = 0xA8; // 預設為 D 暫存器
                Value = value;
            }
        }

        /// <summary>
        /// 非同步隨機寫入多個不連續地址
        /// </summary>
        public async Task RandomWriteAsync(List<WriteTarget> targets)
        {
            // 💡 1. 請求進入單線道，如果定時讀取正在執行，這裡會乖乖排隊
            await _semaphore.WaitAsync();            // 1. 建立非同步連線 (帶有自訂逾時控制)
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(_ip, _port);
                var timeoutTask = Task.Delay(_timeoutMs);

                // 看連線先完成還是逾時先完成
                if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                {
                    throw new TimeoutException($"連線至 PLC ({_ip}:{_port}) 逾時。");
                }

                // 如果連線工作發生異常（如 Port 沒開被拒絕）則拋出錯誤
                if (connectTask.IsFaulted)
                {
                    throw connectTask.Exception?.InnerException ?? new Exception("無法連線至 PLC。");
                }

                // 2. 取得網路資料流並設定傳輸逾時
                using var stream = client.GetStream();
                stream.ReadTimeout = _timeoutMs;
                stream.WriteTimeout = _timeoutMs;

                // 3. 組裝 3E 隨機寫入訊框
                byte[] requestFrame = BuildRandomWriteFrame(targets);

                // 4. 非同步發送請求
                await stream.WriteAsync(requestFrame, 0, requestFrame.Length);

                // 5. 非同步接收回應
                byte[] responseBuffer = new byte[11]; // 隨機寫入回應固定為 11 位元組
                int bytesRead = 0;

                // 確保完整讀取 11 個位元組（避免網路拆包問題）
                while (bytesRead < 11)
                {
                    int read = await stream.ReadAsync(responseBuffer, bytesRead, responseBuffer.Length - bytesRead);
                    if (read == 0) throw new IOException("PLC 提前中斷通訊。");
                    bytesRead += read;
                }

                // 6. 解析 PLC 回應碼
                ushort endCode = BitConverter.ToUInt16(responseBuffer, 9);
                if (endCode != 0)
                {
                    throw new Exception($"PLC 回報異常，錯誤碼: 0x{endCode:X4}");
                }
            }
            finally
            {
                // 7. 無論成功或失敗，都要釋放鎖，讓其他請求可以進入
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 組建 3E 訊框 - 隨機寫入
        /// </summary>
        private byte[] BuildRandomWriteFrame(List<WriteTarget> targets)
        {
            int targetBytesLength = targets.Count * 6;
            byte[] frame = new byte[17 + targetBytesLength];

            // 基礎標頭 (Subheader ~ Destination station)
            frame[0] = 0x50; frame[1] = 0x00;
            frame[2] = 0x00; frame[3] = 0xFF;
            frame[4] = 0xFF; frame[5] = 0x03;
            frame[6] = 0x00;

            // 請求資料長度
            ushort totalRequestLength = (ushort)(8 + targetBytesLength);
            frame[7] = (byte)(totalRequestLength & 0xFF);
            frame[8] = (byte)((totalRequestLength >> 8) & 0xFF);

            // 監控計時器 (4秒) 與指令碼 (0x1402)
            frame[9] = 0x10; frame[10] = 0x00;
            frame[11] = 0x02; frame[12] = 0x14;
            frame[13] = 0x00; frame[14] = 0x00; // Subcommand

            frame[15] = (byte)targets.Count; // Word 點數
            frame[16] = 0x00;               // DWord 點數

            int currentIdx = 17;
            foreach (var target in targets)
            {
                frame[currentIdx] = (byte)(target.Address & 0xFF);
                frame[currentIdx + 1] = (byte)((target.Address >> 8) & 0xFF);
                frame[currentIdx + 2] = (byte)((target.Address >> 16) & 0xFF);
                frame[currentIdx + 3] = target.DeviceCode;
                frame[currentIdx + 4] = (byte)(target.Value & 0xFF);
                frame[currentIdx + 5] = (byte)((target.Value >> 8) & 0xFF);
                currentIdx += 6;
            }

            return frame;
        }
        /// <summary>
        /// 非同步批量讀取連續的字組暫存器 (D)
        /// </summary>
        /// <param name="startAddress">起始地址 (例如 100 代表 D100)</param>
        /// <param name="wordLength">要讀取的連續字組(Word)點數</param>
        /// <returns>讀取到的 short 陣列</returns>
        public async Task<short[]> ReadAsync(int startAddress, ushort wordLength)
        {
            // 💡 1. 請求進入單線道，如果別人在用就排隊等待
            await _semaphore.WaitAsync();
            try
            {
                // 1. 建立非同步連線 (帶有自訂逾時控制)
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(_ip, _port);
                var timeoutTask = Task.Delay(_timeoutMs);
                if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                {
                    throw new TimeoutException($"連線至 PLC ({_ip}:{_port}) 逾時。");
                }

                if (connectTask.IsFaulted)
                {
                    throw connectTask.Exception?.InnerException ?? new Exception("無法連線至 PLC。");
                }

                // 2. 取得網路資料流並設定傳輸逾時
                using var stream = client.GetStream();
                stream.ReadTimeout = _timeoutMs;
                stream.WriteTimeout = _timeoutMs;

                // 3. 組裝 3E 批量讀取訊框
                byte[] requestFrame = BuildReadDRegisterFrame(startAddress, wordLength);

                // 4. 非同步發送讀取請求
                await stream.WriteAsync(requestFrame, 0, requestFrame.Length);

                // 5. 計算預期的回應總位元組數
                // 3E 回應基礎標頭(11位元組) + 數據區 (每個 Word 佔 2 位元組)
                int expectedResponseLength = 11 + (wordLength * 2);
                byte[] responseBuffer = new byte[expectedResponseLength];
                int bytesRead = 0;

                // 6. 非同步接收回應（確保完整讀取預期長度）
                while (bytesRead < expectedResponseLength)
                {
                    int read = await stream.ReadAsync(responseBuffer, bytesRead, responseBuffer.Length - bytesRead);
                    if (read == 0) throw new IOException("PLC 提前中斷通訊。");
                    bytesRead += read;
                }

                // 7. 解析結束代碼 (End Code)
                ushort endCode = BitConverter.ToUInt16(responseBuffer, 9);
                if (endCode != 0)
                {
                    throw new Exception($"PLC 回報讀取異常，錯誤碼: 0x{endCode:X4}");
                }

                // 8. 將回應的位元組資料轉換為 short[] 陣列
                short[] resultData = new short[wordLength];
                int dataIndex = 11; // 資料從第 11 位元組開始
                for (int i = 0; i < wordLength; i++)
                {
                    resultData[i] = BitConverter.ToInt16(responseBuffer, dataIndex + (i * 2));
                }

                return resultData;
            }
            finally
            {
                // 9. 無論成功或失敗，都要釋放鎖，讓其他請求可以進入
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 組建 3E 訊框 - 批量讀取
        /// </summary>
        private byte[] BuildReadDRegisterFrame(int startAddress, ushort length)
        {
            byte[] frame = new byte[21]; // 讀取請求固定為 21 位元組

            // 基礎標頭 (Subheader ~ Destination station)
            frame[0] = 0x50; frame[1] = 0x00; // 3E Request
            frame[2] = 0x00;                  // Network No.
            frame[3] = 0xFF;                  // PC No.
            frame[4] = 0xFF; frame[5] = 0x03; // Module I/O No.
            frame[6] = 0x00;                  // Destination station No.

            // 請求資料長度 (監視計時器 2B + 後續所有指令長度 10B = 12B)
            frame[7] = 0x0C; frame[8] = 0x00;

            // 監視計時器 (4秒)
            frame[9] = 0x10; frame[10] = 0x00;

            // 指令碼 (0x0401 - 批量讀取)
            frame[11] = 0x01; frame[12] = 0x04;

            // 子指令码 (0x0000 - 字組單位)
            frame[13] = 0x00; frame[14] = 0x00;

            // 起始地址 (3 位元組，小端序)
            frame[15] = (byte)(startAddress & 0xFF);
            frame[16] = (byte)((startAddress >> 8) & 0xFF);
            frame[17] = (byte)((startAddress >> 16) & 0xFF);

            // 軟元件代碼 (D 暫存器固定為 0xA8)
            frame[18] = 0xA8;

            // 讀取長度 (Word 點數)
            frame[19] = (byte)(length & 0xFF);
            frame[20] = (byte)((length >> 8) & 0xFF);

            return frame;
        }
        /// <summary>
        /// 【安全執行緒】單點控制 M 線圈的 ON / OFF (位元單位)
        /// </summary>
        /// <param name="mAddress">M 線圈編號 (例如 10 代表 M10)</param>
        /// <param name="turnOn">true 代表 ON (1), false 代表 OFF (0)</param>
        public async Task WriteSingleRelayAsync(int mAddress, bool turnOn)
        {
            // 💡 與讀寫 D 暫存器共用同一個鎖，確保此時不會與背景讀取衝突
            await _semaphore.WaitAsync();

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(_ip, _port);
                var timeoutTask = Task.Delay(_timeoutMs);

                if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                    throw new TimeoutException($"連線至 PLC 逾時。");

                if (connectTask.IsFaulted)
                    throw connectTask.Exception?.InnerException ?? new Exception("無法連線至 PLC。");

                using var stream = client.GetStream();
                stream.ReadTimeout = _timeoutMs;
                stream.WriteTimeout = _timeoutMs;

                // 組建專屬的位元單點寫入訊框
                byte[] requestFrame = BuildSingleBitWriteFrame(mAddress, turnOn);

                await stream.WriteAsync(requestFrame, 0, requestFrame.Length);

                byte[] responseBuffer = new byte[11];
                int bytesRead = 0;

                while (bytesRead < 11)
                {
                    int read = await stream.ReadAsync(responseBuffer, bytesRead, responseBuffer.Length - bytesRead);
                    if (read == 0) throw new IOException("PLC 提前中斷通訊。");
                    bytesRead += read;
                }

                ushort endCode = BitConverter.ToUInt16(responseBuffer, 9);
                if (endCode != 0) throw new Exception($"PLC 回報線圈控制異常，錯誤碼: 0x{endCode:X4}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 組建 3E 訊框 - 批量寫入 (位元單位，用來精準控制單個線圈)
        /// </summary>
        private byte[] BuildSingleBitWriteFrame(int mAddress, bool turnOn)
        {
            // 點數為 1 點時，訊框固定長度為 22 位元組
            byte[] frame = new byte[22];

            // 1~5. 基礎標頭
            frame[0] = 0x50; frame[1] = 0x00; // 3E Request
            frame[2] = 0x00;                  // Network No.
            frame[3] = 0xFF;                  // PC No.
            frame[4] = 0xFF; frame[5] = 0x03; // Module I/O No.
            frame[6] = 0x00;                  // Destination station No.

            // 6. 請求資料長度 (監控計時器 2B + 指令 2B + 子指令 2B + 地址 3B + 代碼 1B + 點數 2B + 數據 1B = 13B)
            frame[7] = 0x0D; frame[8] = 0x00;

            // 7. 監視計時器 (4秒)
            frame[9] = 0x10; frame[10] = 0x00;

            // 8. 指令碼 (0x1401 - 批量寫入)
            frame[11] = 0x01; frame[12] = 0x14;

            // 9. 💡 子指令碼改為 0x0001 (代表以位元 Bit 為單位進行操作)
            frame[13] = 0x01; frame[14] = 0x00;

            // 10. 起始地址 (3 位元組，小端序)
            frame[15] = (byte)(mAddress & 0xFF);
            frame[16] = (byte)((mAddress >> 8) & 0xFF);
            frame[17] = (byte)((mAddress >> 16) & 0xFF);

            // 11. 💡 軟元件代碼改為 0x90 (M 線圈的代碼)
            frame[18] = 0x90;

            // 12. 軟元件點數 (控制 1 點線圈)
            frame[19] = 0x01; frame[20] = 0x00;

            // 13. 💡 寫入數據：在位元通訊中，高 4 位元代表第一點。0x10 代表 ON，0x00 代表 OFF
            frame[21] = turnOn ? (byte)0x10 : (byte)0x00;

            return frame;
        }
    }
}
