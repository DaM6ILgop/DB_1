using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace DB_App
{
    public partial class Form1 : Form
    {

        private SqlConnection connection;
        SqlDataAdapter adapter = null;
        DataTable table = null; 

        private const string connectionString = "Data Source=AzurLine;Initial Catalog=CarDealer2;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                adapter = new SqlDataAdapter("SELECT * FROM Cars", connection);
                table = new DataTable();

                table.Clear();

                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }

        }

        private void Open_Sales_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                adapter = new SqlDataAdapter("SELECT * FROM Sales", connection);
                table = new DataTable();

                table.Clear();

                adapter.Fill(table);
                dataGridView1.DataSource = table;

                MessageBox.Show("Подключение к базе данных успешно установлено");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
            finally 
            {
                connection.Close();
            }
        }

        /*Добавление в таблицу Производители & Модели*/
        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Вставка данных в таблицу CarDealer
                    using (SqlCommand command = new SqlCommand("INSERT INTO CarDealer (Developers_name, Developers_country, Contacts, Terms_of_supply) OUTPUT INSERTED.ID_developer VALUES (@Name, @Country, @Contacts, @Terms)", connection))
                    {
                        command.Parameters.AddWithValue("@Name", textBox1.Text);
                        command.Parameters.AddWithValue("@Country", textBox2.Text);
                        command.Parameters.AddWithValue("@Contacts", textBox3.Text);
                        command.Parameters.AddWithValue("@Terms", comboBox1.Text);

                        // Получение ID_developer, который был автоматически сгенерирован при вставке
                        int ID_developer = (int)command.ExecuteScalar();

                        // Получение ID_developer из таблицы CarDealer
                        //Проверка id на повторение
                        using (SqlCommand getIdCommand = new SqlCommand("SELECT TOP 1 ID_developer FROM CarDealer ORDER BY ID_developer DESC", connection))
                        {
                            // Получение последнего добавленного ID_developer
                            ID_developer = (int)getIdCommand.ExecuteScalar();
                        }

                        // Вставка данных в таблицу Модели, используя полученное значение ID_developer
                        using (SqlCommand modelCommand = new SqlCommand("INSERT INTO CarModels (ID_developer, Model_name) OUTPUT INSERTED.ID_vehicle_model VALUES (@ID_developer, @Model_name)", connection))
                        {
                            modelCommand.Parameters.AddWithValue("@ID_developer", ID_developer);
                            modelCommand.Parameters.AddWithValue("@Model_name", textBox5.Text);

                            // Получение ID_vehicle_model, который был автоматически сгенерирован при вставке в CarModels
                            int ID_vehicle_model = (int)modelCommand.ExecuteScalar();

                            textBox5.Clear();
                        }

                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();

                    }
                }
                catch (Exception ex)
                {
                    // Обработка исключения
                    MessageBox.Show(ex.Message);
                }
                finally 
                {
                    connection.Close();    
                }
            }
        }

        private void Save_To_DB_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string vinNumber = Convert.ToString(row.Cells["VIN_vehicle_number"].Value);
                        DateTime year = Convert.ToDateTime(row.Cells["Year_of_vehicle"].Value); // Используем DateTime для поля Year_of_vehicle
                        string color = Convert.ToString(row.Cells["Car_color"].Value);
                        string engineType = Convert.ToString(row.Cells["Engine_type"].Value);
                        decimal price = Convert.ToDecimal(row.Cells["Car_price"].Value);
                        int idVehicleModel = Convert.ToInt32(row.Cells["ID_vehicle_model"].Value);

                        // Обновление данных в таблице Cars
                        using (SqlCommand command = new SqlCommand("UPDATE Cars SET Year_of_vehicle = @Year, Car_color = @Color, Engine_type = @EngineType, Car_price = @Price WHERE VIN_vehicle_number = @VIN", connection))
                        {
                            command.Parameters.AddWithValue("@VIN", vinNumber);
                            command.Parameters.AddWithValue("@Year", year);
                            command.Parameters.AddWithValue("@Color", color);
                            command.Parameters.AddWithValue("@EngineType", engineType);
                            command.Parameters.AddWithValue("@Price", price);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Данные успешно обновлены в таблице Cars.");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void Save_to_CarTabel_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                string vinNumber = textBox4.Text;
                DateTime year = dateTimePicker1.Value;
                string color = textBox6.Text;
                string engineType = textBox8.Text;
                decimal price = decimal.Parse(textBox9.Text);

                // Получение последнего ID_vehicle_model из таблицы CarModels
                using (SqlCommand command = new SqlCommand("SELECT TOP 1 ID_vehicle_model FROM CarModels ORDER BY ID_vehicle_model DESC", connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int idVehicleModel = reader.GetInt32(0);
                        reader.Close(); // Закрытие DataReader

                        // Вставка данных в таблицу Cars, включая ID_vehicle_model
                        using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Cars (VIN_vehicle_number, ID_vehicle_model, Year_of_vehicle, Car_color, Engine_type, Car_price) VALUES (@VIN, @IDVehicleModel, @Year, @Color, @EngineType, @Price)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@VIN", vinNumber);
                            insertCommand.Parameters.AddWithValue("@IDVehicleModel", idVehicleModel);
                            insertCommand.Parameters.AddWithValue("@Year", year);
                            insertCommand.Parameters.AddWithValue("@Color", color);
                            insertCommand.Parameters.AddWithValue("@EngineType", engineType);
                            insertCommand.Parameters.AddWithValue("@Price", price);

                            insertCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("Данные успешно сохранены в таблицу Cars.");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: не найдено ни одной модели автомобиля в таблице CarModels.");
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        //////////////////////////////////////////////////////////////////
        private void Open_mainList_btn_Click(object sender, EventArgs e)
        {
            LoadData("SELECT * FROM Cars");
        }

        private void LoadData(string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();

                    table.Clear();

                    adapter.Fill(table);
                    dataGridView1.DataSource = table;

                    MessageBox.Show("Подключение к базе данных успешно установлено");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
                }
            }
        }

       

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int rowIndex = dataGridView1.SelectedRows[0].Index;
                string vinNumberToDelete = dataGridView1.Rows[rowIndex].Cells["VIN_vehicle_number"].Value.ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Находим ID_vehicle_model и ID_developer для удаляемой записи
                        using (SqlCommand getIdCommand = new SqlCommand("SELECT c.ID_vehicle_model, m.ID_developer FROM Cars c INNER JOIN CarModels m ON c.ID_vehicle_model = m.ID_vehicle_model WHERE c.VIN_vehicle_number = @VinNumber", connection))
                        {
                            getIdCommand.Parameters.AddWithValue("@VinNumber", vinNumberToDelete);
                            SqlDataReader reader = getIdCommand.ExecuteReader();

                            if (reader.Read())
                            {
                                int idVehicleModel = reader.GetInt32(0);
                                int idDeveloper = reader.GetInt32(1);
                                reader.Close(); // Закрытие DataReader

                                // Удаление записей из таблицы Cars и CarModels
                                using (SqlCommand deleteCarsCommand = new SqlCommand("DELETE FROM Cars WHERE VIN_vehicle_number = @VinNumber", connection))
                                using (SqlCommand deleteModelsCommand = new SqlCommand("DELETE FROM CarModels WHERE ID_vehicle_model = @IdVehicleModel", connection))
                                {
                                    deleteCarsCommand.Parameters.AddWithValue("@VinNumber", vinNumberToDelete);
                                    deleteModelsCommand.Parameters.AddWithValue("@IdVehicleModel", idVehicleModel);

                                    deleteCarsCommand.ExecuteNonQuery();
                                    deleteModelsCommand.ExecuteNonQuery();
                                }

                                // Проверка существования других записей с тем же ID_developer в таблице CarModels
                                using (SqlCommand checkExistenceCommand = new SqlCommand("SELECT COUNT(*) FROM CarModels WHERE ID_developer = @IdDeveloper", connection))
                                {
                                    checkExistenceCommand.Parameters.AddWithValue("@IdDeveloper", idDeveloper);
                                    int count = (int)checkExistenceCommand.ExecuteScalar();

                                    // Удаление записи из таблицы CarDealer, если она не связана с другими моделями
                                    if (count == 0)
                                    {
                                        using (SqlCommand deleteDealerCommand = new SqlCommand("DELETE FROM CarDealer WHERE ID_developer = @IdDeveloper", connection))
                                        {
                                            deleteDealerCommand.Parameters.AddWithValue("@IdDeveloper", idDeveloper);
                                            deleteDealerCommand.ExecuteNonQuery();
                                        }
                                    }
                                }

                                MessageBox.Show("Запись успешно удалена из таблицы Cars, CarModels и CarDealer.");
                            }
                            else
                            {
                                MessageBox.Show("Ошибка: не найдена запись с указанным VIN в таблице Cars.");
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при удалении записи: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления.");
            }
        }
        /////////////////////////////////////////////////////////////////\
    }
}
