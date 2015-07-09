﻿using OceanAirdrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeTracker.Data;

namespace TimeTracker.Dialogs
{
    public partial class EditTimeRecord : Form
    {
        public enum Mode { AddMode, EditMode, DeleteMode }; Mode m_mode;

        public TimeSheetDetailData m_data = null;

        public List<TimerType> m_timerList = null;

        public EditTimeRecord(TimeSheetDetailData data, Mode mode)
        {
            InitializeComponent();

            m_data = data;
            m_mode = mode;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxPMONumber.Text == "")
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.None;
                    MessageBox.Show("Please Select Time Tracker");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                if (m_mode == Mode.AddMode)
                {
                    // insert statement
                    sb.AppendLine("INSERT INTO time_sheet ( date, work_start_time, work_end_time, pmo_number, mins_accrued ) VALUES ( ");
                    sb.AppendLine(string.Format("'{0}'", textBoxDate.Text));
                    sb.AppendLine(string.Format(",'{0}'", textBoxWorkStartDate.Text));
                    sb.AppendLine(string.Format(",'{0}'", textBoxWorkEndTime.Text));
                    sb.AppendLine(string.Format(",'{0}'", textBoxPMONumber.Text));
                    sb.AppendLine(string.Format(",'{0}'", textBoxTimeAccrued.Text));
                    sb.AppendLine(");");
                }

                if (m_mode == Mode.EditMode)
                {
                    // update statment
                    sb.AppendLine("UPDATE time_sheet SET ");
                    sb.AppendLine(string.Format("date = '{0}'", textBoxDate.Text));
                    sb.AppendLine(string.Format(",work_start_time = '{0}'", textBoxWorkStartDate.Text));
                    sb.AppendLine(string.Format(",work_end_time = '{0}'", textBoxWorkEndTime.Text));
                    sb.AppendLine(string.Format(",pmo_number = '{0}'", textBoxPMONumber.Text));
                    sb.AppendLine(string.Format(",mins_accrued = '{0}'", textBoxTimeAccrued.Text));
                    sb.AppendLine(string.Format("WHERE work_id = '{0}'", textBoxWorkId.Text));
                }

                if (m_mode == Mode.DeleteMode)
                {
                    DialogResult d = MessageBox.Show("Are you sure you want to delete this time record?", "You Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (d == DialogResult.No)
                    {
                        this.DialogResult = System.Windows.Forms.DialogResult.None;
                        return;
                    }

                    sb.AppendLine("DELETE FROM time_sheet");
                    sb.AppendLine(string.Format("WHERE work_id = '{0}'", textBoxWorkId.Text));
                }

                LocalSqllite.ExecSQLCommand(sb.ToString());

                MainForm.m_handle.UpdateTotalTimeWorkedToday();

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        void PopulateTimeBox()
        {
            m_timerList = DBHelper.GetTimerList();

            comboBoxDescription.Items.Clear();

            foreach (var data in m_timerList)
            {
                comboBoxDescription.Items.Add(data.desc);
            }
        }

        private void EditTimeRecord_Load(object sender, EventArgs e)
        {
            PopulateTimeBox();

            comboBoxDescription.SelectedItem = m_data.m_desc;

            labelError.Hide();

            SetupDialogMode();
        }

        private void SetupDialogMode()
        {
            if (m_mode == Mode.AddMode)
            {
                this.Text = "Add Time Record";
            }
            if (m_mode == Mode.EditMode)
            {
                this.Text = "Edit Time Record";
            }
            if (m_mode == Mode.DeleteMode)
            {
                this.Text = "Delete Time Record";

                textBoxWorkId.Enabled = false;
                textBoxDate.Enabled = false;
                textBoxWorkStartDate.Enabled = false;
                textBoxWorkEndTime.Enabled = false;
                textBoxPMONumber.Enabled = false;
                textBoxTimeAccrued.Enabled = false;
                textBoxFriendlyTime.Enabled = false;
                comboBoxDescription.Enabled = false;

                buttonOK.Text = "Delete";
            }
        }

        private void SetFriendlyTime()
        {
            try
            {
                DateTime dtStart = Convert.ToDateTime(textBoxWorkStartDate.Text);
                DateTime dtEnd = Convert.ToDateTime(textBoxWorkEndTime.Text);

                TimeSpan duration = dtEnd.Subtract(dtStart);

                textBoxFriendlyTime.Text = TimeSpanToFriendlyString(duration);
            }
            catch (Exception ex)
            {
            }
        }

        string TimeSpanToFriendlyString(TimeSpan ts)
        {
            return string.Format("{0}:{1}:{2}", ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
        }

        private void StartOrEndDateModified()
        {
            try
            {
                labelError.Hide();

                DateTime dtStart = Convert.ToDateTime(textBoxWorkStartDate.Text);
                DateTime dtEnd = Convert.ToDateTime(textBoxWorkEndTime.Text);

                TimeSpan duration = dtEnd.Subtract(dtStart);

                if (duration.TotalMinutes < 0)
                    throw new Exception("invalid"); // move to exception block

                textBoxTimeAccrued.Text = duration.TotalMinutes.ToString();

                SetFriendlyTime();

                buttonOK.Enabled = true;
            }
            catch (Exception ex)
            {
                buttonOK.Enabled = false;
                labelError.Show();
                labelError.Text = "Invalid Time Selection";
            }
        }

        private void textBoxWorkStartDate_TextChanged(object sender, EventArgs e)
        {
            StartOrEndDateModified();

        }

        private void textBoxWorkEndTime_TextChanged(object sender, EventArgs e)
        {
            StartOrEndDateModified();

        }

        private void textBoxTimeAccrued_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxDescription_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach ( var item in m_timerList)
            {
                if ( item.desc == comboBoxDescription.SelectedItem.ToString() )
                {
                    textBoxPMONumber.Text = item.pmo_num;
                    break;
                }
            }
        }
    }
}
