using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Services;
using System.Text.RegularExpressions;
using System.Xml;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Settings;


namespace Kesco.App.Web.Stores
{
	public class srv : System.Web.Services.WebService
	{
		public srv()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code

		//Required by the Web Services Designer 
		private IContainer components = null;


		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing && components != null )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#endregion

		[WebMethod( Description=@"<br><pre>
������������ ����� � ���� <b>�����</b> � ����������� ������� �������������� <i>fn_ReplaceKeySymbols</i>,<i>fn_SplitWords</i>.
������������ �������� - <i>���������� ��������� �������</i>.
���������:
  out int    id           - ��� ���������� ������ (���� ���������� ���������� ������� <b>=1</b>.)
      string searchText   - ������ ������.
      string searchParams - ��������� ������ � ������(<i>param1=val1&amp;param2=val2...</i>, �� ������ ������ ����������� ��������� ���������� <b>StoreType</b>,<b>StoreKeeper</b>,<b>StoreManager</b>,
                            <b>StoreResource</b>,<b>storeexcept</b>)
</pre>
" )]
		public int Search( int id, string searchText, string searchParams )
		{
			string sqlWhere = "";
			int rCount = 0;
			id = 0;
			SqlDataAdapter da = null;
			DataTable dt = new DataTable( "result" );

			try
			{
				if( searchParams != "" )
				{
					string[] p = searchParams.Split( '&' );
					string[] v;
					for( int i = 0; i < p.Length; i++ )
					{
						v = p[ i ].Split( '=' );
						switch( v[ 0 ].ToLower() )
						{
							case "storekeeper":
								if( v[ 1 ] != "" ) sqlWhere += " AND ������������=" + v[ 1 ];
								break;
							case "storemanager":
								if( v[ 1 ] != "" ) sqlWhere += " AND ����������������=" + v[ 1 ];
								break;
							case "storeresource":
								if( v[ 1 ] != "" ) sqlWhere += " AND ����������=" + v[ 1 ];
								break;
							case "storetype":
								if( v[ 1 ] != "" )
								{
									if( !Regex.IsMatch( v[ 1 ], "^\\d+(,\\d+){0,}$" ) ) continue;
									sqlWhere += " AND ������������� IN(" + v[ 1 ] + ")";
								}
								break;
							case "storeactual":
								if( v[ 1 ] != "" ) sqlWhere += " AND ���������=" + v[ 1 ];
								break;
							case "storeexcept":
								if( v[ 1 ] != "" ) sqlWhere += " AND ��������� <>" + v[ 1 ];
								break;

						}
					}
				}

				string sql = @"
SET @searchText = RTRIM(LTRIM(��������������.dbo.fn_ReplaceKeySymbols(��������������.dbo.fn_SplitWords(@searchText))))
WHILE CHARINDEX('  ',@searchText) > 0 SET @searchText = REPLACE(@searchText,'  ',' ')

SET @searchText='%' + REPLACE(@searchText,' ','% ') + '%'
SELECT ���������
FROM �����������.dbo.vw������ ������
	LEFT OUTER JOIN �����������.dbo.vw���� ��������� ON ���������.������� = ������.������������
WHERE (����� +' ' + IBAN + ' ' +���������.������) LIKE @searchText " + sqlWhere + @"
				";

				da = new SqlDataAdapter( sql, Config.DS_person );
				da.SelectCommand.Parameters.Add( "@searchText", searchText );
				da.Fill( dt );
				rCount = dt.Rows.Count;
				if( rCount == 1 )
					id = (int) dt.Rows[ 0 ][ "���������" ];
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "������ ��� ������ �������", ex, da.SelectCommand );

			}
			return rCount;
		}


		[WebMethod( Description=@"<br><pre>
�������� ��������(�����) ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
" )]
		public string GetCaption( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( "SET @Store=ISNULL((SELECT IBAN+(CASE WHEN LEN(IBAN)>0 AND LEN(�����)>0 THEN '/' ELSE '' END)+����� ����� FROM �����������.dbo.vw������ WHERE ���������=" + id + "),'-1')" );
				cm.Parameters.Add( "@Store", System.Data.SqlDbType.VarChar, 100 );
				cm.Parameters[ "@Store" ].Direction = System.Data.ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
				cm.Connection.Open();
				cm.ExecuteNonQuery();
				retVal = cm.Parameters[ "@Store" ].Value.ToString();
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "������ ��������� �������� ������", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			if( retVal.Equals( "-1" ) ) retVal = "#" + id.ToString();
			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
�������� ��������� ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
" )]
		public string GetCaption_Keeper( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( @"SET @Store=ISNULL((SELECT ����.������ 
FROM �����������.dbo.vw������ ������ INNER JOIN 
�����������.dbo.vw���� ���� ON ������.������������=����.�������
WHERE ���������=" + id + "),'-1')" );
				cm.Parameters.Add( "@Store", System.Data.SqlDbType.VarChar, 100 );
				cm.Parameters[ "@Store" ].Direction = System.Data.ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
				cm.Connection.Open();
				cm.ExecuteNonQuery();
				retVal = cm.Parameters[ "@Store" ].Value.ToString();
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "������ ��������� �������� ��������� �� ���� ������", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			if( retVal.Equals( "-1" ) ) retVal = "#" + id.ToString();
			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
�������� ��������� ������ �� ���� ������.
���������:
  int id - ��� ������.
		</pre>
" )]
		public string GetCaption_Keeper1( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( @"SELECT ����.������, �����
										FROM �����������.dbo.vw������ ������ INNER JOIN 
										�����������.dbo.vw���� ���� ON ������.������������=����.�������
										WHERE ���������=@ID" );
				cm.Parameters.Add( "@ID", id );
                cm.Connection = new SqlConnection(Config.DS_person);
				cm.Connection.Open();
				SqlDataReader dr = cm.ExecuteReader();

				if( dr.Read() )
					retVal = dr[ 0 ].ToString() + "--" + dr[ 1 ].ToString();
				else
					retVal = "#" + id.ToString();
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "������ ��������� �������� ��������� ������", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
�������� ���������� � ������ �� ���� ������.
���������:
      int       id                    - ��� ������.
  out string    storeKeeper           - ���������� ��� ���������.
  out string    storeManager          - ���������� ��� �������������.
  out string    storeType             - ���������� ��� ���� ������.
  out string    storeResource         - ���������� ��� �������.

		</pre>
" )]
		public void GetInfo( int id, out string storeKeeper, out string storeManager, out string storeType, out string storeResource )
		{
			DataTable dt = new DataTable( "info" );
			SqlDataAdapter da = null;
			string sql = "";
			try
			{
				sql = @"
					SELECT 
						������������ StoreKeeper,
						���������������� StoreManager,
						������������� StoreType,
						���������� StoreResource
					FROM
						�����������.dbo.vw������ ������
					WHERE ���������=@Id";
                da = new SqlDataAdapter(sql, Config.DS_person);
				da.SelectCommand.Parameters.Add( "@Id", id );
				da.Fill( dt );
				storeKeeper = storeManager = storeType = storeResource = "";
				if( dt.Rows.Count != 0 )
				{
					storeKeeper = dt.Rows[ 0 ][ "StoreKeeper" ].ToString();
					storeManager = dt.Rows[ 0 ][ "StoreManager" ].ToString();
					storeType = dt.Rows[ 0 ][ "StoreType" ].ToString();
					storeResource = dt.Rows[ 0 ][ "StoreResource" ].ToString();
				}
			}
			catch( Exception ex )
			{
				throw new DetailedException( "������ ��������� ���������� �� ������", ex, da.SelectCommand );

			}
		}

	}
}