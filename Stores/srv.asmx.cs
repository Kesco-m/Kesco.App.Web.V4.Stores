using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Services;
using Kesco.Lib.Entities;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Stores
{
    public class srv : WebService
    {
        public srv()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        [WebMethod(Description = @"<br><pre>
������������ ����� � ���� <b>�����</b> � ����������� ������� �������������� <i>fn_ReplaceKeySymbols</i>,<i>fn_SplitWords</i>.
������������ �������� - <i>���������� ��������� �������</i>.
���������:
  out int    id           - ��� ���������� ������ (���� ���������� ���������� ������� <b>=1</b>.)
      string searchText   - ������ ������.
      string searchParams - ��������� ������ � ������(<i>param1=val1&amp;param2=val2...</i>, �� ������ ������ ����������� ��������� ���������� <b>StoreType</b>,<b>StoreKeeper</b>,<b>StoreManager</b>,
                            <b>StoreResource</b>,<b>storeexcept</b>)
</pre>
")]
        public int Search(int id, string searchText, string searchParams)
        {
            //TODO: ���������� �� sp_������_�����

            var sqlWhere = "";
            var rCount = 0;
            id = 0;
            SqlDataAdapter da = null;
            var dt = new DataTable("result");

            try
            {
                if (!string.IsNullOrEmpty(searchParams))
                {
                    var p = searchParams.Split('&');
                    foreach (var t in p)
                    {
                        var v = t.Split('=');
                        switch (v[0].ToLower())
                        {
                            case "storekeeper":
                                if (!string.IsNullOrEmpty(v[1])) sqlWhere += " AND ������������=" + v[1];
                                break;
                            case "storemanager":
                                if (!string.IsNullOrEmpty(v[1])) sqlWhere += " AND ����������������=" + v[1];
                                break;
                            case "storeresource":
                                if (!string.IsNullOrEmpty(v[1])) sqlWhere += " AND ����������=" + v[1];
                                break;
                            case "storetype":
                                if (!string.IsNullOrEmpty(v[1]))
                                {
                                    if (!Regex.IsMatch(v[1], "^\\d+(,\\d+){0,}$")) continue;
                                    sqlWhere += " AND ������������� IN(" + v[1] + ")";
                                }

                                break;
                            case "storeactual":
                                if (!string.IsNullOrEmpty(v[1]))
                                {
                                    if (v[1] == "1")
                                        sqlWhere +=
                                            " AND ISNULL(��,'19800101')< GETDATE() AND ISNULL(��-1,'20500101') > GETDATE()";
                                    else
                                        sqlWhere += " AND ��-1 < GETDATE()";
                                }

                                break;
                            case "storeexcept":
                                if (!string.IsNullOrEmpty(v[1])) sqlWhere += " AND ��������� <>" + v[1];
                                break;
                        }
                    }
                }

                da = new SqlDataAdapter(string.Format("{0} {1}" , SQLQueries.SELECT_����������������������, sqlWhere) , Config.DS_person);
                da.SelectCommand.Parameters.AddWithValue("@searchText", searchText);
                da.Fill(dt);
                rCount = dt.Rows.Count;
                if (rCount == 1)
                    id = (int) dt.Rows[0]["���������"];
            }

            catch (Exception ex)
            {
                throw new DetailedException("������ ��� ������ �������", ex, da.SelectCommand);
            }
            finally
            {
                if (da != null) da.Dispose();
                dt.Dispose();
            }

            return rCount;
        }


        [WebMethod(Description = @"<br><pre>
�������� ��������(�����) ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
")]
        public string GetCaption(int id)
        {
            string retVal;
            SqlCommand cm = null;
            try
            {
                cm = new SqlCommand(
                    "SET @Store=ISNULL((SELECT IBAN+(CASE WHEN LEN(IBAN)>0 AND LEN(�����)>0 THEN '/' ELSE '' END)+����� ����� FROM �����������.dbo.vw������ WHERE ���������=" +
                    id + "),'-1')");
                cm.Parameters.Add("@Store", SqlDbType.VarChar, 100);
                cm.Parameters["@Store"].Direction = ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
                cm.Connection.Open();
                cm.ExecuteNonQuery();
                retVal = cm.Parameters["@Store"].Value.ToString();
            }

            catch (Exception ex)
            {
                throw new DetailedException("������ ��������� �������� ������", ex, cm);
            }
            finally
            {
                if (cm != null)
                {
                    cm.Connection.Close();
                    cm.Dispose();
                }

            }

            if (retVal.Equals("-1")) retVal = "#" + id;
            return retVal;
        }



        [WebMethod(Description = @"<br><pre>
�������� ��������� ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
")]
        public string GetCaption_Keeper(int id)
        {
            string retVal;
            SqlCommand cm = null;
            try
            {
                cm = new SqlCommand(@"SET @Store=ISNULL((SELECT ����.������ 
FROM �����������.dbo.vw������ ������ INNER JOIN 
�����������.dbo.vw���� ���� ON ������.������������=����.�������
WHERE ���������=" + id + "),'-1')");
                cm.Parameters.Add("@Store", SqlDbType.VarChar, 100);
                cm.Parameters["@Store"].Direction = ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
                cm.Connection.Open();
                cm.ExecuteNonQuery();
                retVal = cm.Parameters["@Store"].Value.ToString();
            }

            catch (Exception ex)
            {
                throw new DetailedException("������ ��������� �������� ��������� �� ���� ������", ex, cm);
            }
            finally
            {
                if (cm != null)
                {
                    cm.Connection.Close();
                    cm.Dispose();
                }
            }

            if (retVal.Equals("-1")) retVal = "#" + id;
            return retVal;
        }


        [WebMethod(Description = @"<br><pre>
�������� ��������� ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
")]
        public string GetCaption_Keeper1(int id)
        {
            string retVal;
            SqlCommand cm = null;
            try
            {
                cm = new SqlCommand(@"SELECT ����.������, �����
										FROM �����������.dbo.vw������ ������ INNER JOIN 
										�����������.dbo.vw���� ���� ON ������.������������=����.�������
										WHERE ���������=@ID");
                cm.Parameters.AddWithValue("@ID", id);
                cm.Connection = new SqlConnection(Config.DS_person);
                cm.Connection.Open();
                var dr = cm.ExecuteReader();

                if (dr.Read())
                    retVal = dr[0] + "--" + dr[1];
                else
                    retVal = "#" + id;
            }

            catch (Exception ex)
            {
                throw new DetailedException("������ ��������� �������� ��������� ������", ex, cm);
            }
            finally
            {
                if (cm != null)
                {
                    cm.Connection.Close();
                    cm.Dispose();
                }
            }

            return retVal;
        }


        [WebMethod(Description = @"<br><pre>
�������� ���������� � ������ �� ���� ������.
���������:
      int       id                    - ��� ������.
  out string    storeKeeper           - ���������� ��� ���������.
  out string    storeManager          - ���������� ��� �������������.
  out string    storeType             - ���������� ��� ���� ������.
  out string    storeResource         - ���������� ��� �������.

		</pre>
")]
        public void GetInfo(int id, out string storeKeeper, out string storeManager, out string storeType,
            out string storeResource)
        {
            var dt = new DataTable("info");
            SqlDataAdapter da = null;
            try
            {
                const string sql = @"
					SELECT 
						������������ StoreKeeper,
						���������������� StoreManager,
						������������� StoreType,
						���������� StoreResource
					FROM
						�����������.dbo.vw������ ������
					WHERE ���������=@Id";
                da = new SqlDataAdapter(sql, Config.DS_person);
                da.SelectCommand.Parameters.AddWithValue("@Id", id);
                da.Fill(dt);
                storeKeeper = storeManager = storeType = storeResource = "";
                if (dt.Rows.Count != 0)
                {
                    storeKeeper = dt.Rows[0]["StoreKeeper"].ToString();
                    storeManager = dt.Rows[0]["StoreManager"].ToString();
                    storeType = dt.Rows[0]["StoreType"].ToString();
                    storeResource = dt.Rows[0]["StoreResource"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw new DetailedException("������ ��������� ���������� �� ������", ex, da.SelectCommand);
            }
            finally
            {
                if (da != null) da.Dispose();
                dt.Dispose();
            }
        }

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;


        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }


        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}