using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace turanskii_students
{
    public partial class Form1 : Form
    {
        private List<Student> students = new List<Student>();
        private bool isDataChanged = false;
        private readonly string[] allowedDomains = { "yandex.ru", "gmail.com", "icloud.com" };

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Set minimum date for birth date picker
            dtpBirthDate.MinDate = new DateTime(1992, 1, 1);
            dtpBirthDate.MaxDate = DateTime.Today;
            dtpBirthDate.Value = new DateTime(2000, 1, 1);

            // Initialize course filter
            for (int i = 1; i <= 6; i++)
            {
                cmbFilterCourse.Items.Add(i);
            }

            // Wire up event handlers
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSortLastName.Click += BtnSortLastName_Click;
            btnSortGroup.Click += BtnSortGroup_Click;
            btnSortCourse.Click += BtnSortCourse_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbFilterCourse.SelectedIndexChanged += Filter_Changed;
            cmbFilterGroup.SelectedIndexChanged += Filter_Changed;
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            loadToolStripMenuItem.Click += LoadToolStripMenuItem_Click;
            exportToolStripMenuItem.Click += ExportToolStripMenuItem_Click;
            importToolStripMenuItem.Click += ImportToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            this.FormClosing += Form1_FormClosing;
        }

        private void UpdateStudentList()
        {
            lvStudents.Items.Clear();
            foreach (var student in students)
            {
                var item = new ListViewItem(new[]
                {
                    student.LastName,
                    student.FirstName,
                    student.MiddleName,
                    student.Course.ToString(),
                    student.Group,
                    student.BirthDate.ToShortDateString(),
                    student.Email
                });
                lvStudents.Items.Add(item);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Пожалуйста, введите фамилию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMiddleName.Text))
            {
                MessageBox.Show("Пожалуйста, введите отчество", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMiddleName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtGroup.Text))
            {
                MessageBox.Show("Пожалуйста, введите группу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtGroup.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Пожалуйста, введите email", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }

            // Validate email format
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Неверный формат email. Используйте домены: yandex.ru, gmail.com, icloud.com", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email) return false;

                string domain = email.Split('@')[1];
                return allowedDomains.Contains(domain);
            }
            catch
            {
                return false;
            }
        }

        private void ClearInputFields()
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtGroup.Clear();
            txtEmail.Clear();
            nudCourse.Value = 1;
            dtpBirthDate.Value = new DateTime(2000, 1, 1);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var student = new Student(
                txtLastName.Text,
                txtFirstName.Text,
                txtMiddleName.Text,
                (int)nudCourse.Value,
                txtGroup.Text,
                dtpBirthDate.Value,
                txtEmail.Text
            );

            students.Add(student);
            isDataChanged = true;
            UpdateStudentList();
            UpdateGroupFilter();
            ClearInputFields();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lvStudents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите студента для редактирования", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ValidateInput()) return;

            int index = lvStudents.SelectedItems[0].Index;
            students[index] = new Student(
                txtLastName.Text,
                txtFirstName.Text,
                txtMiddleName.Text,
                (int)nudCourse.Value,
                txtGroup.Text,
                dtpBirthDate.Value,
                txtEmail.Text
            );

            isDataChanged = true;
            UpdateStudentList();
            UpdateGroupFilter();
            ClearInputFields();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvStudents.SelectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите студента для удаления", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранного студента?", 
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int index = lvStudents.SelectedItems[0].Index;
                students.RemoveAt(index);
                isDataChanged = true;
                UpdateStudentList();
                UpdateGroupFilter();
                ClearInputFields();
            }
        }

        private void BtnSortLastName_Click(object sender, EventArgs e)
        {
            students = students.OrderBy(s => s.LastName).ToList();
            UpdateStudentList();
        }

        private void BtnSortGroup_Click(object sender, EventArgs e)
        {
            students = students.OrderBy(s => s.Group).ToList();
            UpdateStudentList();
        }

        private void BtnSortCourse_Click(object sender, EventArgs e)
        {
            students = students.OrderBy(s => s.Course).ToList();
            UpdateStudentList();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            var filteredStudents = students.Where(s => s.LastName.ToLower().Contains(searchText)).ToList();
            ApplyFilters(filteredStudents);
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters(students);
        }

        private void ApplyFilters(List<Student> sourceStudents)
        {
            var filteredStudents = sourceStudents;

            if (cmbFilterCourse.SelectedItem != null)
            {
                int course = (int)cmbFilterCourse.SelectedItem;
                filteredStudents = filteredStudents.Where(s => s.Course == course).ToList();
            }

            if (cmbFilterGroup.SelectedItem != null)
            {
                string group = cmbFilterGroup.SelectedItem.ToString();
                filteredStudents = filteredStudents.Where(s => s.Group == group).ToList();
            }

            lvStudents.Items.Clear();
            foreach (var student in filteredStudents)
            {
                var item = new ListViewItem(new[]
                {
                    student.LastName,
                    student.FirstName,
                    student.MiddleName,
                    student.Course.ToString(),
                    student.Group,
                    student.BirthDate.ToShortDateString(),
                    student.Email
                });
                lvStudents.Items.Add(item);
            }
        }

        private void UpdateGroupFilter()
        {
            var groups = students.Select(s => s.Group).Distinct().ToList();
            cmbFilterGroup.Items.Clear();
            foreach (var group in groups)
            {
                cmbFilterGroup.Items.Add(group);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = "json",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string json = JsonConvert.SerializeObject(students, Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, json);
                isDataChanged = false;
                MessageBox.Show("Данные успешно сохранены", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    students = JsonConvert.DeserializeObject<List<Student>>(json);
                    UpdateStudentList();
                    UpdateGroupFilter();
                    isDataChanged = false;
                    MessageBox.Show("Данные успешно загружены", "Успех", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var csv = new StringBuilder();
                csv.AppendLine("Фамилия,Имя,Отчество,Курс,Группа,Дата рождения,Email");

                foreach (var student in students)
                {
                    csv.AppendLine($"{student.LastName},{student.FirstName},{student.MiddleName}," +
                        $"{student.Course},{student.Group},{student.BirthDate.ToShortDateString()},{student.Email}");
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                MessageBox.Show("Данные успешно экспортированы", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    if (lines.Length < 2)
                    {
                        throw new Exception("Файл не содержит данных");
                    }

                    var newStudents = new List<Student>();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] fields = lines[i].Split(',');
                        if (fields.Length != 7)
                        {
                            throw new Exception($"Неверный формат данных в строке {i + 1}");
                        }

                        newStudents.Add(new Student(
                            fields[0],
                            fields[1],
                            fields[2],
                            int.Parse(fields[3]),
                            fields[4],
                            DateTime.Parse(fields[5]),
                            fields[6]
                        ));
                    }

                    students = newStudents;
                    UpdateStudentList();
                    UpdateGroupFilter();
                    isDataChanged = true;
                    MessageBox.Show("Данные успешно импортированы", "Успех", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте файла: {ex.Message}", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isDataChanged)
            {
                var result = MessageBox.Show("Есть несохраненные изменения. Сохранить перед выходом?", 
                    "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveToolStripMenuItem_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
