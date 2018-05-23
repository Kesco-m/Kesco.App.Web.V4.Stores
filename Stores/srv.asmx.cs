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
Осуществляет поиск в поле <b>Склад</b> с применением функции преобразования <i>fn_ReplaceKeySymbols</i>,<i>fn_SplitWords</i>.
Возвращаемое значение - <i>количество найденных складов</i>.
Параметры:
  out int    id           - код найденного склада (если количество найденнных складов <b>=1</b>.)
      string searchText   - строка поиска.
      string searchParams - параметры поиска в фомате(<i>param1=val1&amp;param2=val2...</i>, на данный момент реализована обработка параметров <b>StoreType</b>,<b>StoreKeeper</b>,<b>StoreManager</b>,
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
								if( v[ 1 ] != "" ) sqlWhere += " AND КодХранителя=" + v[ 1 ];
								break;
							case "storemanager":
								if( v[ 1 ] != "" ) sqlWhere += " AND КодРаспорядителя=" + v[ 1 ];
								break;
							case "storeresource":
								if( v[ 1 ] != "" ) sqlWhere += " AND КодРесурса=" + v[ 1 ];
								break;
							case "storetype":
								if( v[ 1 ] != "" )
								{
									if( !Regex.IsMatch( v[ 1 ], "^\\d+(,\\d+){0,}$" ) ) continue;
									sqlWhere += " AND КодТипаСклада IN(" + v[ 1 ] + ")";
								}
								break;
							case "storeactual":
								if( v[ 1 ] != "" ) sqlWhere += " AND Действует=" + v[ 1 ];
								break;
							case "storeexcept":
								if( v[ 1 ] != "" ) sqlWhere += " AND КодСклада <>" + v[ 1 ];
								break;

						}
					}
				}

				string sql = @"
SET @searchText = RTRIM(LTRIM(Инвентаризация.dbo.fn_ReplaceKeySymbols(Инвентаризация.dbo.fn_SplitWords(@searchText))))
WHILE CHARINDEX('  ',@searchText) > 0 SET @searchText = REPLACE(@searchText,'  ',' ')

SET @searchText='%' + REPLACE(@searchText,' ','% ') + '%'
SELECT КодСклада
FROM Справочники.dbo.vwСклады Склады
	LEFT OUTER JOIN Справочники.dbo.vwЛица Хранители ON Хранители.КодЛица = Склады.КодХранителя
WHERE (Склад +' ' + IBAN + ' ' +Хранители.Кличка) LIKE @searchText " + sqlWhere + @"
				";

				da = new SqlDataAdapter( sql, Config.DS_person );
				da.SelectCommand.Parameters.Add( "@searchText", searchText );
				da.Fill( dt );
				rCount = dt.Rows.Count;
				if( rCount == 1 )
					id = (int) dt.Rows[ 0 ][ "КодСклада" ];
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "Ошибка при поиске складов", ex, da.SelectCommand );

			}
			return rCount;
		}


		[WebMethod( Description=@"<br><pre>
Получить название(номер) склада по коду склада.
Параметры:
  int id - код склада.
		</pre>
" )]
		public string GetCaption( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( "SET @Store=ISNULL((SELECT IBAN+(CASE WHEN LEN(IBAN)>0 AND LEN(Склад)>0 THEN '/' ELSE '' END)+Склад Склад FROM Справочники.dbo.vwСклады WHERE КодСклада=" + id + "),'-1')" );
				cm.Parameters.Add( "@Store", System.Data.SqlDbType.VarChar, 100 );
				cm.Parameters[ "@Store" ].Direction = System.Data.ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
				cm.Connection.Open();
				cm.ExecuteNonQuery();
				retVal = cm.Parameters[ "@Store" ].Value.ToString();
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "Ошибка получения названия склада", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			if( retVal.Equals( "-1" ) ) retVal = "#" + id.ToString();
			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
Получить хранителя склада по коду склада.
Параметры:
  int id - код склада.
		</pre>
" )]
		public string GetCaption_Keeper( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( @"SET @Store=ISNULL((SELECT Лица.Кличка 
FROM Справочники.dbo.vwСклады Склады INNER JOIN 
Справочники.dbo.vwЛица Лица ON Склады.КодХранителя=Лица.КодЛица
WHERE КодСклада=" + id + "),'-1')" );
				cm.Parameters.Add( "@Store", System.Data.SqlDbType.VarChar, 100 );
				cm.Parameters[ "@Store" ].Direction = System.Data.ParameterDirection.Output;
                cm.Connection = new SqlConnection(Config.DS_person);
				cm.Connection.Open();
				cm.ExecuteNonQuery();
				retVal = cm.Parameters[ "@Store" ].Value.ToString();
			}
			
			catch( Exception ex )
			{
				throw new DetailedException( "Ошибка получения названия хранителя по коду склада", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			if( retVal.Equals( "-1" ) ) retVal = "#" + id.ToString();
			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
Получить хранителя склада по коду склада.
Параметры:
  int id - код склада.
		</pre>
" )]
		public string GetCaption_Keeper1( int id )
		{
			string retVal;
			SqlCommand cm = null;
			try
			{
				cm = new SqlCommand( @"SELECT Лица.Кличка, Склад
										FROM Справочники.dbo.vwСклады Склады INNER JOIN 
										Справочники.dbo.vwЛица Лица ON Склады.КодХранителя=Лица.КодЛица
										WHERE КодСклада=@ID" );
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
				throw new DetailedException( "Ошибка получения названия хранителя склада", ex, cm );

			}
			finally
			{
				cm.Connection.Close();
			}

			return retVal;
		}


		[WebMethod( Description=@"<br><pre>
Получить информацию о складе по коду склада.
Параметры:
      int       id                    - код склада.
  out string    storeKeeper           - возвращает код хранителя.
  out string    storeManager          - возвращает код распорядителя.
  out string    storeType             - возвращает код типа склада.
  out string    storeResource         - возвращает код ресурса.

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
						КодХранителя StoreKeeper,
						КодРаспорядителя StoreManager,
						КодТипаСклада StoreType,
						КодРесурса StoreResource
					FROM
						Справочники.dbo.vwСклады Склады
					WHERE КодСклада=@Id";
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
				throw new DetailedException( "Ошибка получения информации по складу", ex, da.SelectCommand );

			}
		}

	}
}