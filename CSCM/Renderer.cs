using System;
using System.Runtime.InteropServices;
using System.Timers;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace CSAC
{
    internal class Renderer : Overlay
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vkey);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type; // 0 for input type mouse
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        bool AutoClickerON = false;
        int RepeatTimes = 0;
        int RepeatOption = 0;
        int MiliInput = 100;
        int SecInput = 0;
        int MinInput = 0;
        int HouInput = 0;
        private System.Timers.Timer clickTimer;
        private int clicksRemaining;

        string[] mouseButtons = { "Left", "Middle", "Right" }; // Mouse button options
        int selectedMouseButton = 0; // Default to "Left"

        string[] clickTypes = { "Single", "Double" }; // Click type options
        int selectedClickType = 0; // Default to "Single"

        protected override void Render()
        {
            if (GetAsyncKeyState(0x75) < 0)
            {
                AutoClickerON = !AutoClickerON;
                Thread.Sleep(200); // Prevent toggling too quickly
                if (AutoClickerON)
                {
                    StartClicking();
                }
                else
                {
                    StopClicking();
                }
            }

            ImGui.Begin("AutoClicker");
            ImGui.Text("Copper Stairs Autoclicker");
            ImGui.RadioButton("Autoclicker = " + AutoClickerON + " (F6 to toggle)", AutoClickerON);

            // Numeric inputs for time
            ImGui.InputInt("Milliseconds", ref MiliInput);
            ImGui.InputInt("Seconds", ref SecInput);
            ImGui.InputInt("Minutes", ref MinInput);
            ImGui.InputInt("Hours", ref HouInput);

            // Clamp values to ensure they are non-negative
            if (MiliInput < 0) MiliInput = 0;
            if (SecInput < 0) SecInput = 0;
            if (MinInput < 0) MinInput = 0;
            if (HouInput < 0) HouInput = 0;

            // Radio buttons for repeat option
            ImGui.RadioButton("Repeat Until Stopped", ref RepeatOption, 0);
            ImGui.RadioButton("Repeat", ref RepeatOption, 1);
            ImGui.SameLine();
            ImGui.InputInt("", ref RepeatTimes);
            if (RepeatTimes < 0) RepeatTimes = 0; // Clamp to prevent negative values

            ImGui.SameLine();
            ImGui.Text("Times");

            // Dropdown for selecting the mouse button
            ImGui.Text("Select Mouse Button:");
            ImGui.Combo("Mouse Button", ref selectedMouseButton, mouseButtons, mouseButtons.Length);

            // Dropdown for selecting the click type
            ImGui.Text("Select Click Type:");
            ImGui.Combo("Click Type", ref selectedClickType, clickTypes, clickTypes.Length);

            ImGui.Spacing();
            

            if (ImGui.Button("Exit"))
            {
                Environment.Exit(0);
            }
            ImGui.End();
        }

        private void StartClicking()
        {
            if (clickTimer == null)
            {
                clicksRemaining = RepeatOption == 0 ? int.MaxValue : RepeatTimes;
                clickTimer = new System.Timers.Timer(CalculateDelay());
                clickTimer.Elapsed += OnTimedEvent;
                clickTimer.AutoReset = true;
                clickTimer.Start();
            }
        }

        private void StopClicking()
        {
            if (clickTimer != null)
            {
                clickTimer.Stop();
                clickTimer.Dispose();
                clickTimer = null;
            }
        }

        private int CalculateDelay()
        {
            return MiliInput + (SecInput * 1000) + (MinInput * 60000) + (HouInput * 3600000);
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (clicksRemaining > 0)
            {
                PerformClick();
                clicksRemaining--;
            }
            else if (RepeatOption == 1) // If it's set to repeat, stop when it reaches the limit
            {
                StopClicking();
            }
        }

        private void PerformClick()
        {
            INPUT[] input = new INPUT[1];
            uint downFlag = 0;
            uint upFlag = 0;

            switch (selectedMouseButton)
            {
                case 0: // Left
                    downFlag = MOUSEEVENTF_LEFTDOWN;
                    upFlag = MOUSEEVENTF_LEFTUP;
                    break;
                case 1: // Middle
                    downFlag = MOUSEEVENTF_MIDDLEDOWN;
                    upFlag = MOUSEEVENTF_MIDDLEUP;
                    break;
                case 2: // Right
                    downFlag = MOUSEEVENTF_RIGHTDOWN;
                    upFlag = MOUSEEVENTF_RIGHTUP;
                    break;
            }

            // Mouse down
            input[0] = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dwFlags = downFlag
                }
            };
            SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));

            // Mouse up
            input[0].mi.dwFlags = upFlag;
            SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));

            // For double clicks, perform the same again
            if (selectedClickType == 1) // Double click
            {
                // Mouse down
                input[0] = new INPUT
                {
                    type = INPUT_MOUSE,
                    mi = new MOUSEINPUT
                    {
                        dwFlags = downFlag
                    }
                };
                SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));

                // Mouse up
                input[0].mi.dwFlags = upFlag;
                SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));
            }
        }
    }
}
