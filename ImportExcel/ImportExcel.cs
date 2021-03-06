using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AccidentForecast
{  
    public enum Area
    {
        [Description("0")]
        Obl = 0,
        [Description("Волов")]
        Vol=1,
        [Description("Гряз")]
        Grz=2,
        [Description("Данков")]
        Dan = 3,
        [Description("Добрин")]
        Dobrin = 4,
        [Description("Добро")]
        Dobrov = 5,
        [Description("Долгорук")]
        Dolg = 6,
        [Description("Елецкий")]
        Elck = 7,
        [Description("Задон")]
        Zadon = 8,
        [Description("Измал")]
        Izmal = 9,
        [Description("Красн")]
        Krsn = 10,
        [Description("Лебед")]
        Leb = 11,
        [Description("Лев")]
        Lev = 12,
        [Description("Липецкий")]
        Lipeck = 13,
        [Description("Стано")]
        Stan = 14,
        [Description("Тербу")]
        Terb = 15,
        [Description("Усман")]
        Usm = 16,
        [Description("Хлев")]
        Hlev = 17,
        [Description("Чаплыг")]
        Chapl = 18,
        [Description("г. Елец")]
        El = 19,
        [Description("г. Липецк")]
        Lip = 20,
    }
    static class EnumExtensions
    {
        static public string Description(this Area value)
        {
            var attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(false)
                .FirstOrDefault(a=> a is DescriptionAttribute) as DescriptionAttribute;

            return attribute !=null ? attribute.Description : value.ToString();
        }
    }

    public class ImportExcel
    {
        //Факторы
        public static void LoadExcelFactors(string listName, string pathName)
        {
            DataTable dtexcel = new DataTable();

            //string constr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\_LSTU\4 КУРС\1 семестр\Модельки\ЛР1\Лаб1_Тербуны\Абсолютные\Тербуны_влажность.xlsm;Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            string constr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;'",pathName);
            using (OleDbConnection conn = new OleDbConnection(constr))
            {
                conn.Open();

                OleDbDataAdapter daexcel = new OleDbDataAdapter(String.Format("Select * from [{0}$]",listName), conn);
                dtexcel.Locale = CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
                conn.Close();
            }
            string connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=TestDBAccidents;Data Source=DESKTOP-PPEQF8T";
            string sql = "insert into Factors values (@A, @B, @C, @D,@E,@F,@G, @H,20)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (DataRow row in dtexcel.Rows)
                {
                    if (row["Дата"].ToString() == "")
                        break;
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@A", row["Дата"]);
                    cmd.Parameters.AddWithValue("@B", row[11]);
                    cmd.Parameters.AddWithValue("@C", row[12]);
                    cmd.Parameters.AddWithValue("@D", row[13]);
                    cmd.Parameters.AddWithValue("@E", row[14]);
                    cmd.Parameters.AddWithValue("@F", row[15]);
                    cmd.Parameters.AddWithValue("@G", row[16]);
                    cmd.Parameters.AddWithValue("@H", row[17]);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

       //ЖКХ
        public static void LoadExcelHaCS(string listName, string pathName)
        {
            DataTable dtexcel = new DataTable();

            string constr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES; IMEX=1'",pathName);

            using (OleDbConnection conn = new OleDbConnection(constr))
            {
                conn.Open();

                OleDbDataAdapter daexcel = new OleDbDataAdapter(String.Format("Select * from [{0}$]",listName), conn);
                dtexcel.Locale = CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
                conn.Close();
            }

            dtexcel.Rows.RemoveAt(0);
            dtexcel.Rows.RemoveAt(0);



            //      dtexcel.Rows.RemoveAt(0);
            string connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=TestDBAccidents;Data Source=DESKTOP-PPEQF8T";
            string sqlPower = "insert into PowerSupply values (@A, @B, @C)";
            string sqlWater = "insert into WaterSupply values (@D, @E, @F)";
            string sqlHeat = "insert into HeatSupply values (@G, @H, @I)";
            int i = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                DateTime chekdate = DateTime.Now;
                int itemInd = 0;
                foreach (DataRow row in dtexcel.Rows)
                {
                    if (row[1].ToString() == "")
                        break;
                    SqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = sqlPower;
                    if (row[0].ToString() == "")
                        row[0] = chekdate;
                    else
                        chekdate = Convert.ToDateTime(row[0]);
                    cmd.Parameters.AddWithValue("@A", row[0]);
                    cmd.Parameters.AddWithValue("@B", Convert.ToInt32(row[1]));
                    foreach (Area item in Enum.GetValues(typeof(Area)))
                    {

                        if (row[2].ToString().IndexOf(item.Description()) > -1) // СДЕЛАТЬ В ОСТАЛЬНЫХ ТАКЖЕ
                            itemInd = Convert.ToInt32(item);
                    }
                    cmd.Parameters.AddWithValue("@C", itemInd);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = sqlHeat;
                    if (row[0].ToString() == "")
                        row[0] = chekdate;
                    else
                        chekdate = Convert.ToDateTime(row[0]);
                    cmd.Parameters.AddWithValue("@G", row[0]);
                    cmd.Parameters.AddWithValue("@H", Convert.ToInt32(row[3]));
                    foreach (Area item in Enum.GetValues(typeof(Area)))
                    {

                        if (row[4].ToString().IndexOf(item.Description()) > -1) // СДЕЛАТЬ В ОСТАЛЬНЫХ ТАКЖЕ
                            itemInd = Convert.ToInt32(item);
                    }
                    cmd.Parameters.AddWithValue("@I", itemInd);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = sqlWater;
                    if (row[0].ToString() == "")
                        row[0] = chekdate;
                    else
                        chekdate = Convert.ToDateTime(row[0]);
                    cmd.Parameters.AddWithValue("@D", row[0]);
                    cmd.Parameters.AddWithValue("@E", Convert.ToInt32(row[5]));
                    foreach (Area item in Enum.GetValues(typeof(Area)))
                    {
                        if (row[6].ToString().IndexOf(item.Description()) > -1) // СДЕЛАТЬ В ОСТАЛЬНЫХ ТАКЖЕ
                            itemInd = Convert.ToInt32(item);
                    }
                    cmd.Parameters.AddWithValue("@F", itemInd);
                    cmd.ExecuteNonQuery();

                    /*      ++i;
                          if(i>=5)
                       break;*/

                }
                conn.Close();
            }
        }
        //ДТП
        public static void LoadExcelAccident(string listName, string pathName)
        {
            DataTable dtexcel = new DataTable();

            string constr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;'",pathName);
            using (OleDbConnection conn = new OleDbConnection(constr))
            {
                conn.Open();

                OleDbDataAdapter daexcel = new OleDbDataAdapter(String.Format("Select * from [{0}$]",listName), conn);
                dtexcel.Locale = CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
                conn.Close();
            }
            dtexcel.Rows.RemoveAt(0);
            dtexcel.Rows.RemoveAt(0);
            string connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=TestDBAccidents;Data Source=DESKTOP-PPEQF8T";
            string sql = "insert into Accident values (@A, @B, @C, @D)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                DateTime chekdate = DateTime.Now;
                int itemInd = 0;
                foreach (DataRow row in dtexcel.Rows)
                {
                    if (row[1].ToString() == "")
                        break;
                    SqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = sql;
                    if (row[0].ToString() == "")
                        row[0] = chekdate;
                    else
                        chekdate = Convert.ToDateTime(row[0]);
                    cmd.Parameters.AddWithValue("@A", row[0]);
                    cmd.Parameters.AddWithValue("@B", row[2]);
                    cmd.Parameters.AddWithValue("@C", row[3]);
                    foreach (Area item in Enum.GetValues(typeof(Area)))
                    {

                        if (row[1].ToString().IndexOf(item.Description()) > -1) // СДЕЛАТЬ В ОСТАЛЬНЫХ ТАКЖЕ
                            itemInd = Convert.ToInt32(item);
                    }
                    cmd.Parameters.AddWithValue("@D", itemInd);
                    cmd.ExecuteNonQuery();
                    //  break;
                }
                conn.Close();
            }
        }
       //Пожары
        public static void LoadExcelFires(string listName, string pathName)
        {
            DataTable dtexcel = new DataTable();

            string constr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;'",pathName);
            using (OleDbConnection conn = new OleDbConnection(constr))
            {
                conn.Open();

                OleDbDataAdapter daexcel = new OleDbDataAdapter(String.Format("Select * from [{0}$]",listName), conn);
                dtexcel.Locale = CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
                conn.Close();
            }
            dtexcel.Rows.RemoveAt(0);
            //      dtexcel.Rows.RemoveAt(0);
            string connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=TestDBAccidents;Data Source=DESKTOP-PPEQF8T";
            string sqlFires = "insert into Fires values (@A, @B, @C, @D, @M); SELECT @ID=SCOPE_IDENTITY()";
            string sqlObject = "insert into ObjectFires values (1,@E,@F)";
            string sqlGetFiresIdent = "select @@IDENTITY";


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                DateTime chekdate = DateTime.Now;

                foreach (DataRow row in dtexcel.Rows)
                {
                    int itemInd = -1;
                    if (row[1].ToString() == "")
                        break;



                    SqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = sqlFires;
                    SqlParameter output = cmd.Parameters.Add("@ID", SqlDbType.Int);
                    output.Direction = ParameterDirection.Output;
                    if (row[0].ToString() == "")
                    {
                        //  chekdate = new DateTime(1899, 12, 30).AddDays(Convert.ToDouble(row[0]));
                        row[0] = chekdate.ToString();
                    }
                    else
                    {
                        try
                        {
                            if (!(row[0] is DateTime))
                                chekdate = new DateTime(1899, 12, 30).AddDays(Convert.ToDouble(row[0]));
                            else
                                chekdate = Convert.ToDateTime(row[0]);
                        }
                        catch
                        {
                            chekdate = Convert.ToDateTime(row[0]);
                        }
                        row[0] = chekdate.ToString();
                        //  chekdate = Convert.ToDateTime(row[0]);
                    }
                    cmd.Parameters.AddWithValue("@A", Convert.ToDateTime(row[0]));
                    cmd.Parameters.AddWithValue("@B", row[3]);
                    cmd.Parameters.AddWithValue("@M", row[4]);
                    cmd.Parameters.AddWithValue("@C", row[5]);
                    foreach (Area item in Enum.GetValues(typeof(Area)))
                    {

                        if (row[1].ToString().IndexOf(item.Description()) > -1 && Convert.ToInt32(item) != 0) // СДЕЛАТЬ В ОСТАЛЬНЫХ ТАКЖЕ
                            itemInd = Convert.ToInt32(item);
                    }
                    if (itemInd == -1)
                    {
                        if (row[1].ToString().IndexOf("Липецк") > -1)
                            itemInd = 20;
                        else if (row[1].ToString().IndexOf("Елец") > -1)
                            itemInd = 19;
                        /*  else if (row[1].ToString().IndexOf("Грязи") > -1)
                              itemInd = 2;
                          else if (row[1].ToString().IndexOf("Данков") > -1)
                              itemInd = 3;
                          else if (row[1].ToString().IndexOf("Задонск") > -1)
                              itemInd = 8;
                          else if (row[1].ToString().IndexOf("Чаплыгин") > -1)
                              itemInd = 18;
                          else if (row[1].ToString().IndexOf("Тербуны") > -1)
                              itemInd = 15;
                          else if (row[1].ToString().IndexOf("Лев") > -1)
                              itemInd = 12;
                          else if (row[1].ToString().IndexOf("Добринка") > -1)
                              itemInd = 4;
                          else if (row[1].ToString().IndexOf("Усмань") > -1)
                              itemInd = 16;
                          else if (row[1].ToString().IndexOf("Лебедянь") > -1)
                              itemInd = 11;
                          else if (row[1].ToString().IndexOf("Становое") > -1)
                              itemInd = 14;
                          else if (row[1].ToString().IndexOf("Волово") > -1)
                              itemInd = 1;
                          else if (row[1].ToString().IndexOf("Красное") > -1)
                              itemInd = 10;
                          else if (row[1].ToString().IndexOf("Хлевное") > -1)
                              itemInd = 17;*/
                    }
                    if (itemInd == -1)
                        itemInd = 0;

                    cmd.Parameters.AddWithValue("@D", itemInd);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = sqlObject;

                    cmd.Parameters.AddWithValue("@E", output.Value);
                    cmd.Parameters.AddWithValue("@F", row[2]);

                    cmd.ExecuteNonQuery();

                    /*DataTable dtIndent = new DataTable();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlGetFiresIdent, connectionString);
                    sqlDataAdapter.Fill(dtIndent);*/
                    //int a = Convert.ToInt32(output.Value);



                }
                conn.Close();
            }
        }
    }
}