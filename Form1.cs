using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace BigFiles1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("FileName", "File Name");
            dataGridView1.Columns.Add("Extension", "Extension");
            dataGridView1.Columns.Add("FilePath", "File Path");
            dataGridView1.Columns.Add("Size", "Size (in MB)");

            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick; ;
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var cell = dataGridView1.Rows[e.RowIndex].Cells["FilePath"];
                if (cell?.Value != null)
                {
                    OpenFileLocation(cell.Value.ToString());
                }
            }
        }

        void OpenFileLocation(string filePath)
        {
            try
            {
                Process.Start(Path.GetDirectoryName(filePath));
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }

        public List<FileDetails> SearchResults { get; set; }

        FileSizeProvider fileSizeProvider = new FileSizeProvider();

        private void searchButton_Click(object sender, EventArgs e)
        {
            Initialise();

            long sizeLimit = (long)fileSizeControl.Value * 1024 * 1024;

            fileSizeProvider.Search(rootFolderTextBox.Text, sizeLimit, UpdateResult, DisplayError, SearchCompleted);
        }

        private void DisplayError(string error)
        {
            UpdateStatus(error);
        }

        private void SearchCompleted(bool completed)
        {
            UpdateStatus(completed ? "Search completed" : "Search failed");
            End();
        }

        private void UpdateResult(FileInfo fileInfo)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action<FileInfo>(UpdateResult), fileInfo);
                return;
            }
            var fileDetails = new FileDetails(fileInfo);

            dataGridView1.Rows.Add(
                fileDetails.FileName,
                fileDetails.FileExtn,
                fileDetails.FilePath,
                fileDetails.SizeInMB);

            dataGridView1.AutoResizeColumns();
        }

        private void UpdateStatus(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action<string>(UpdateStatus), message);
                return;
            }
            listBox1.Items.Add(message);
        }

        private void Initialise()
        {
            searchButton.Enabled = false;
            statusStrip1.Visible = true;
            dataGridView1.Rows.Clear();
            listBox1.Items.Clear();
            btnCancel.Enabled = true;

            UpdateStatus($"Search started at {DateTime.Now}...");
        }

        private void End()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(End)); return;
            }
            searchButton.Enabled = true;
            statusStrip1.Visible = false;
            btnCancel.Enabled = false;
            UpdateStatus($"Search finished at {DateTime.Now}...");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            fileSizeProvider.CancelSearch(UpdateStatus);
        }

        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
            {
                e.ContextMenuStrip = contextMenuStrip1;
                e.ContextMenuStrip.Show();
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex != -1 && e.ColumnIndex != -1)
                {
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                    if (dataGridView1.SelectedRows.Count > 1)
                    {
                        contextMenuStrip1.Items[0].Enabled = false;
                        contextMenuStrip1.Items[1].Enabled = false;
                    }
                    else if (dataGridView1.SelectedRows.Count == 1)
                    {
                        contextMenuStrip1.Items[0].Enabled = true;
                        contextMenuStrip1.Items[1].Enabled = true;
                    }
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void deleteAllSelectedItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    try
                    {
                        File.Delete(row.Cells["Location"].Value.ToString());
                        dataGridView1.Rows.RemoveAt(row.Index);
                        dataGridView1.Refresh();
                    }
                    catch (Exception f)
                    {
                        UpdateStatus(f.Message);
                    }
                }
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cell = dataGridView1.SelectedRows[0].Cells["FilePath"];
            if (cell?.Value != null)
            {
                OpenFileLocation(cell.Value.ToString());
            }
        }

        private void copyFolderPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cell = dataGridView1.SelectedRows[0].Cells["FilePath"];
            if (cell?.Value != null)
            {
                CopyFilePathToClipBoard(cell.Value.ToString());
            }
        }

        void CopyFilePathToClipBoard(string filePath)
        {
            Clipboard.SetText(filePath);
        }
    }
}
