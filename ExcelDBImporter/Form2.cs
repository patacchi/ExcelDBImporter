using ExcelDBImporter.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;


namespace ExcelDBImporter
{
    public partial class FrmDataViewer : Form
    {
        private readonly ExcelDbContext dbContextAlias;
        public FrmDataViewer()
        {
            InitializeComponent();
            dbContextAlias = new();
        }

        private void FrmDataViewer_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                //EnsureCreated使うとMigrationの時にタイヘン・・・
                //dbContextAlias.Database.EnsureCreated();
                Tool.RegistAllClassAndPropertys RegistClass = new(typeof(ShShukka).Namespace ?? string.Empty,nameof(ShShukka));
                //Aliasテーブルのキーの存在チェック(ない場合は追加)
                RegistClass.AddAliasNameTableByClassnameAndNamespace(typeof(ShShukka).Namespace ?? string.Empty, nameof(ShShukka));
                BindingList<TableFieldAliasNameList> BindListAlias = new BindingList<TableFieldAliasNameList>();
                dgvUpdater.DataSource = dbContextAlias.TableFieldAliasNameLists
                                                        .Include(dbdata => dbdata.DBcolumn)
                                                        .Where(l => l.DBcolumn.StrClassName == nameof(ShShukka))
                                                        .OrderBy(t => t.DBcolumn.StrClassName)
                                                        .ThenBy(t => t.DBcolumn.StrDBColumnName)
                                                        .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
            }
        }

        private void BtnSaveChanges_Click(object sender, EventArgs e)
        {
            MessageBox.Show(dbContextAlias.SaveChanges().ToString() + " 件更新しました");
        }
    }
}
