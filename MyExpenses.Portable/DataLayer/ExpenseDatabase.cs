//#define PRELOAD
using System;
using System.Collections.Generic;
using System.Linq;
using MyExpenses.Portable.DataLayer.SQLiteBase;
using MyExpenses.Portable.Interfaces;
using MyExpenses.Portable.Models;


namespace MyExpenses.Portable.DataLayer
{
  /// <summary>
  /// TaskDatabase builds on SQLite.Net and represents a specific database, in our case, the Task DB.
  /// It contains methods for retrieval and persistance as well as db creation, all based on the 
  /// underlying ORM.
  /// </summary>
  public class ExpenseDatabase
  {
    static object locker = new object();

    SQLiteConnection database;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDatabase"/> TaskDatabase. 
    /// if the database doesn't exist, it will create the database and all the tables.
    /// </summary>
    /// <param name='path'>
    /// Path.
    /// </param>
    public ExpenseDatabase(SQLiteConnection conn)
    {
      database = conn;
      // create the tables
      database.CreateTable<Expense>();
      
#if PRELOAD
      if (!GetItems<Expense>().Any())
      {
        for (int i = 0; i < 500; i++)
        {
          var expense = new Expense()
          {
            Category = "Uncategorized",
            Billable = true,
            Due = DateTime.Now.AddDays(i),
            Name = "Expense " + i,
            Total = (100 + i).ToString()
          };
          SaveItem(expense);
        }
      }
#endif

    }

    public IEnumerable<T> GetItems<T>() where T : IBusinessEntity, new()
    {
      lock (locker)
      {
        return (from i in database.Table<T>() select i).ToList();
      }
    }

    public T GetItem<T>(int id) where T : IBusinessEntity, new()
    {
      lock (locker)
      {
        return database.Table<T>().FirstOrDefault(x => x.ID == id);
        // Following throws NotSupportedException - thanks aliegeni
        //return (from i in Table<T> ()
        //        where i.ID == id
        //        select i).FirstOrDefault ();
      }
    }

    public int SaveItem<T>(T item) where T : IBusinessEntity
    {
      lock (locker)
      {
        if (item.ID != 0)
        {
          database.Update(item);
          return item.ID;
        }
        else
        {
          return database.Insert(item);
        }
      }
    }

    public int DeleteItem<T>(int id) where T : IBusinessEntity, new()
    {
      lock (locker)
      {
        return database.Delete<T>(new T() { ID = id });
      }
    }
  }
}