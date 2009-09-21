#region Released to Public Domain by eSymmetrix, Inc.

/********************************************************************************
 *   This file is sample code demonstrating Gibraltar integration with PostSharp
 *   
 *   This sample is free software: you have an unlimited rights to
 *   redistribute it and/or modify it.
 *   
 *   This sample is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *   
 *******************************************************************************/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace GSharpDemo
{
    public partial class WorkerUI : UserControl
    {
        private bool m_KeepRunning;
        private Thread m_WorkerThread;
        private int m_WorkerThreadSpeed;
        private int m_WorkerWaitTime;
        private int m_cycleCount;

        private void ComputeWaitTime()
        {
            // workerSpeed comes from m_workerThreadSpeed, which comes from trackBarSpeed, which ranges from 0 to 20
            // This results in a sleep of 10..110ms per iteration resulting in max cylce time of about 2.5 seconds
            m_WorkerWaitTime = 10 + 5 *(20 - m_WorkerThreadSpeed);
        }

        public WorkerUI()
        {
            InitializeComponent();
            trackbarSpeed.Minimum = 0;
            trackbarSpeed.Maximum = 20;
            trackbarSpeed.Value = 20;
            m_WorkerThreadSpeed = trackbarSpeed.Value;
            ComputeWaitTime();
        }

        private void trackbarSpeed_ValueChanged(object sender, EventArgs e)
        {
            m_WorkerThreadSpeed = trackbarSpeed.Value;
            ComputeWaitTime();
        }


        private void HighlightCycleCount()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(HighlightCycleCount));
            }
            else
            {
                lblCycleCount.Text = m_cycleCount.ToString();
                verticalProgressBar1.ProgressInPercentage = 0;
                verticalProgressBar2.ProgressInPercentage = 0;
                verticalProgressBar3.ProgressInPercentage = 0;
                lblCycleCount.ForeColor = Color.Red;
                BackColor = Color.LightYellow;

            }
        }

        private void UnhighlightCycleCount()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(UnhighlightCycleCount));
            }
            else
            {
                lblCycleCount.ForeColor = SystemColors.ControlText;
                BackColor = Color.White;

            }
        }

        public void Start()
        {
            m_KeepRunning = true;
            m_WorkerThread = new Thread(WorkerThread) {IsBackground = true};
            m_WorkerThread.Start();
        }

        private void WorkerThread()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int progress1 = 0;
            int progress2 = 0;
            int progress3 = 0;

            while (m_KeepRunning)
            {
                progress1 += 20;

                if (progress1 > 100)
                {
                    progress1 = 0;
                    progress2 += 20;
                    Trace.WriteLine("First bar has spilled over.");
                }

                if (progress2 > 100)
                {
                    progress2 = 0;
                    progress3 += 20;
                    Trace.TraceInformation("Second bar has spilled over.");
                }

                if (progress3 > 100)
                {
                    progress3 = 0;
                    m_cycleCount++;
                    stopWatch.Stop();
                    Trace.TraceWarning("Third bar has spilled over. Cycle-time: "
                                       + stopWatch.Elapsed.TotalSeconds.ToString("F2"));

                    HighlightCycleCount();
                    Thread.Sleep(500);
                    UnhighlightCycleCount();

                    Program.TimeSpans.Add(stopWatch.Elapsed);
                    stopWatch.Reset();
                    stopWatch.Start();
                }

                verticalProgressBar1.ProgressInPercentage = progress1;
                verticalProgressBar2.ProgressInPercentage = progress2;
                verticalProgressBar3.ProgressInPercentage = progress3;

                Thread.Sleep(m_WorkerWaitTime);
            }
        }

        public void Stop()
        {
            m_KeepRunning = false; // Stop worker thread
            if (m_WorkerThread != null)
                m_WorkerThread.Join(500);
        }

        private void WorkerUI_Resize(object sender, EventArgs e)
        {
            Width = 140;
        }
    }
}