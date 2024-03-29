﻿using ExcelDBImporter.Context;
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
using ExcelDBImporter.Tool;
using System.Diagnostics;


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
            dgvUpdater.AutoGenerateColumns = false;
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
                SortableBindingList<TableFieldAliasNameList> blistAlias = new(dbContextAlias.TableFieldAliasNameLists
                                                        .Include(dbdata => dbdata.DBcolumn)
                                                        .Where(l => l.DBcolumn.StrClassName == nameof(ShShukka))
                                                        .OrderBy(t => t.DBcolumn.StrClassName)
                                                        .ThenBy(t => t.DBcolumn.StrDBColumnName)
                                                        .ToList());
                dgvUpdater.AutoGenerateColumns = true;
                dgvUpdater.DataSource = blistAlias;
                dgvUpdater.AutoResizeColumns();
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
