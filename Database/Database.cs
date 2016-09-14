﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallStore
{
    class Database
    {
        //const string CONN_STRING = @"Data Source=ipd8.database.windows.net;Initial Catalog=store;Integrated Security=False;User ID=ipd8abbott;Password=Abbott2000;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        const string CONN_STRING = @"Data Source=ali-ipd8.database.windows.net;Initial Catalog=store;Integrated Security=False;User ID=ali-ipd8;Password=torabi-2016;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private SqlConnection conn;
        public Database()
        {
            conn = new SqlConnection(CONN_STRING);
            conn.Open();//it may throw exception
        }

        //*******************Transaction
        public bool Transaction_OrderSubmit(OrderSummary orderSum, int CashierId, int customerId)
        {
            SqlTransaction transaction = conn.BeginTransaction(); 
                
            try
            {

                AddOrderSummary(orderSum);
                int orderId = GetOrderSummaryId(orderSum);
                AddAllOrderItem(orderSum.Items);
                foreach (OrderItem item in orderSum.Items)
                {
                    ReduceProductNumberInStuck(item.ProductId, item.NumberOfUnit);
                }
                transaction.Commit();
                return true;
            }
            catch (Exception ex) {
                transaction.Rollback();
                return false;
            }






            return false;
        }
        //  ******************* CRUD methods for Employee ************************ -->

        public bool LoginUser(string user, string pass, out bool isManager)
        {
            SqlCommand cmd = new SqlCommand("SELECT IsManager FROM Employee WHERE UserName=@Username and Password=@Password ", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@Username", user);
            cmd.Parameters.AddWithValue("@Password", pass);

            isManager = false;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                int rowCount = 0;

                if (reader.HasRows)
                {
                    rowCount = 0;
                    while (reader.Read())
                    {
                        rowCount++;
                        isManager = (bool)reader.GetSqlBoolean(reader.GetOrdinal("IsManager"));
                    }

                }
                if (rowCount == 1)
                {

                    return true;
                }
                return false;
            }
        }

        public void addEmployee(Employee e)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Employee (FirstName, LastName,Phone,CellPhone,Address,PostalCode,BirthDate,JobTitle,HireDate,FireDate,Salary,IsPermenant,IsManager,UserName,Password) VALUES (@FirstName, @LastName,@Phone,@CellPhone,@Address,@PostalCode,@BirthDate,@JobTitle,@HireDate,@FireDate,@Salary,@IsPermenant,@IsManager,@UserName,@Password)"))
            {

                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@FirstName", e.FirstName);
                cmd.Parameters.AddWithValue("@LastName", e.LastName);
                cmd.Parameters.AddWithValue("@Phone", e.Phone);
                //   cmd.Parameters.AddWithValue("@CellPhone", e.CellPhone);|
                cmd.Parameters.AddWithValue("@Address", e.Address);
                cmd.Parameters.AddWithValue("@PostalCode", e.PostalCode);
                cmd.Parameters.AddWithValue("@BirthDate", e.BirthDate);
                cmd.Parameters.AddWithValue("@JobTitle", e.JobTitle);
                cmd.Parameters.AddWithValue("@HireDate", e.HireDate);
                cmd.Parameters.AddWithValue("@FireDate", e.FireDate);
                cmd.Parameters.AddWithValue("@Salary", e.Salary);
                cmd.Parameters.AddWithValue("@IsPermenant", e.IsPermenant);
                cmd.Parameters.AddWithValue("@IsManager", e.IsManager);
                cmd.Parameters.AddWithValue("@UserName", e.UserName);
                cmd.Parameters.AddWithValue("@Password", e.Password);

                cmd.ExecuteNonQuery();
            }

        }
        public Employee getEmployeeByUserName(string user)
        {
            Employee e = new Employee();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Employee WHERE UserName=@Username  ", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@Username", user);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {


                if (reader.HasRows)
                {

                    while (reader.Read())
                    {

                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string fname = reader.GetString(reader.GetOrdinal("FirstName"));
                        string lname = reader.GetString(reader.GetOrdinal("LastName"));

                        e.FirstName = fname;
                        e.LastName = lname;
                        e.Id = id;
                        e.UserName = user;

                    }


                }
            }
            return e;
        }

        // <-- ******************* CRUD methods for Employee ************************

        //  ********************** CRUD methods for Customer ************************ -->

        public void addCustomer(Customer c)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Customer (FirstName, LastName,Phone,CellPhone,Address,PostalCode,DiscountPercentage,Reg``isterDate,ModifyDate) VALUES (@FirstName, @LastName,@Phone,@CellPhone,@Address,@PostalCode,@DiscountPercentage,@RegisterDate,@ModifyDate)"))
            {

                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@FirstName", c.FirstName);
                cmd.Parameters.AddWithValue("@LastName", c.LastName);
                cmd.Parameters.AddWithValue("@Phone", c.Phone);
                cmd.Parameters.AddWithValue("@CellPhone", c.CellPhone);
                cmd.Parameters.AddWithValue("@Address", c.Address);
                cmd.Parameters.AddWithValue("@PostalCode", c.PostalCode);
                cmd.Parameters.AddWithValue("@DiscountPercentage", c.DiscountPercentage);
                cmd.Parameters.AddWithValue("@RegisterDate", c.RegisterDate);
                cmd.Parameters.AddWithValue("@ModifyDate", c.ModifyDate);

                cmd.ExecuteNonQuery();
            }

        }

        // <--  ********************** CRUD methods for Customer ************************

        //  ******************* CRUD methods for Product ************************ -->
        public void ReduceProductNumberInStuck(int productId, int numberOfItem)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM Product where ProductId=@ProductId", conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            int numberInStock = 0;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        numberInStock = reader.GetInt32(reader.GetOrdinal("NumberInStock"));
                        break;
                    }
                }
            }


            using (SqlCommand cmd1 = new SqlCommand("UPDATE Product SET NumberInStock=@NumberInStock WHERE ProductId=@ProductId", conn))
            {

                cmd1.CommandType = System.Data.CommandType.Text;
                cmd1.Connection = conn;
                cmd1.Parameters.AddWithValue("@ProductId", productId);
                cmd1.Parameters.AddWithValue("@NumberInStock", numberInStock - numberOfItem);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Product> GetAllProductByNameOrBarcode(string str)
        {
            List<Product> productList = new List<Product>();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Product where ProductName LIKE @ProductName OR Barcode  LIKE @Barcode", conn);

            cmd.Parameters.AddWithValue("@ProductName", "%" + str + "%");
            cmd.Parameters.AddWithValue("@Barcode", "%" + str + "%");
            using (SqlDataReader reader = cmd.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("ProductId"));
                        string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                        int categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                        string barcode = reader.GetString(reader.GetOrdinal("Barcode"));
                        int numberInStock = reader.GetInt32(reader.GetOrdinal("NumberInStock"));
                        decimal purchasePrice = reader.GetDecimal(reader.GetOrdinal("PurchasePrice"));
                        decimal salePrice = reader.GetDecimal(reader.GetOrdinal("SalePrice"));
                        string unit = reader.GetString(reader.GetOrdinal("Unit"));
                        decimal specialDiscount = reader.GetDecimal(reader.GetOrdinal("SpecialDiscount"));

                        Product p = new Product() { Id = id, ProductName = productName, CategoryId = categoryId, Barcode = barcode, NumberInStock = numberInStock, PurchasePrice = purchasePrice, SalePrice = salePrice, Unit = unit, SpecialDiscount = specialDiscount };
                        productList.Add(p);

                    }

                }
            }
            return productList;
        }

        public void AddProductItem(Product p)
        {
            SqlCommand cmd;
            if (p.ProductImage != null && p.ProductImage.Length > 0)
            {
                cmd = new SqlCommand("INSERT INTO Product (ProductName, CategoryId, Barcode, NumberInStock, PurchasePrice, SalePrice, Unit, ProductImage, SpecialDiscount) VALUES (@ProductName, @CategoryId, @Barcode, @NumberInStock, @PurchasePrice, @SalePrice, @Unit, @ProductImage, @SpecialDiscount)", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.Parameters.AddWithValue("@Barcode", p.Barcode);
                cmd.Parameters.AddWithValue("@NumberInStock", p.NumberInStock);
                cmd.Parameters.AddWithValue("@PurchasePrice", p.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", p.SalePrice);
                cmd.Parameters.AddWithValue("@Unit", p.Unit);
                cmd.Parameters.AddWithValue("@ProductImage", p.ProductImage);
                cmd.Parameters.AddWithValue("@SpecialDiscount", p.SpecialDiscount);
            }
            else
            {
                cmd = new SqlCommand("INSERT INTO Product (ProductName, CategoryId, Barcode, NumberInStock, PurchasePrice, SalePrice, Unit, SpecialDiscount) VALUES (@ProductName, @CategoryId, @Barcode, @NumberInStock, @PurchasePrice, @SalePrice, @Unit, @SpecialDiscount)", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.Parameters.AddWithValue("@Barcode", p.Barcode);
                cmd.Parameters.AddWithValue("@NumberInStock", p.NumberInStock);
                cmd.Parameters.AddWithValue("@PurchasePrice", p.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", p.SalePrice);
                cmd.Parameters.AddWithValue("@Unit", p.Unit);
                cmd.Parameters.AddWithValue("@SpecialDiscount", p.SpecialDiscount);
            }

            cmd.ExecuteNonQuery();
        }
        public Product GetProductById(int productId)
        {
            Product p = new Product();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Product WHERE ProductId=@ProductId", conn));
            cmd.Parameters.AddWithValue("@ProductId", productId);
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ProductId"));
                            string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                            int categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                            string barcode = reader.GetString(reader.GetOrdinal("Barcode"));
                            int numberInStock = reader.GetInt32(reader.GetOrdinal("NumberInStock"));
                            decimal purchasePrice = reader.GetDecimal(reader.GetOrdinal("PurchasePrice"));
                            decimal salePrice = reader.GetDecimal(reader.GetOrdinal("SalePrice"));
                            string unit = reader.GetString(reader.GetOrdinal("Unit"));
                            //byte[] productImage = reader.GetValue(reader.GetOrdinal("ProductImage")) as byte[];
                            decimal specialDiscount = reader.GetDecimal(reader.GetOrdinal("SpecialDiscount"));
                            p = new Product() { Id = id, ProductName = productName, CategoryId = categoryId, Barcode = barcode, NumberInStock = numberInStock, PurchasePrice = purchasePrice, SalePrice = salePrice, Unit = unit, ProductImage = productImage, SpecialDiscount = specialDiscount };

                            return p;
                        }
                    }
                }
            }
            return p;
        }
        public List<Product> GetAllProducts()
        {
            List<Product> ProductList = new List<Product>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Product", conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ProductId"));
                            string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                            int categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                            string barcode = reader.GetString(reader.GetOrdinal("Barcode"));
                            int numberInStock = reader.GetInt32(reader.GetOrdinal("NumberInStock"));
                            decimal purchasePrice = reader.GetDecimal(reader.GetOrdinal("PurchasePrice"));
                            decimal salePrice = reader.GetDecimal(reader.GetOrdinal("SalePrice"));
                            string unit = reader.GetString(reader.GetOrdinal("Unit"));
                            byte[] productImage = reader.GetValue(reader.GetOrdinal("ProductImage")) as byte[];
                            decimal specialDiscount = reader.GetDecimal(reader.GetOrdinal("SpecialDiscount"));
                            Product p = new Product() { Id = id, ProductName = productName, CategoryId = categoryId, Barcode = barcode, NumberInStock = numberInStock, PurchasePrice = purchasePrice, SalePrice = salePrice, Unit = unit, ProductImage = productImage, SpecialDiscount = specialDiscount };

                            ProductList.Add(p);
                        }
                    }
                }
            }
            return ProductList;
        }

        public void DeleteProductItemById(int Id)
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM Product WHERE ProductId = " + Id, conn);
            cmd.ExecuteNonQuery();
        }

        public void UpdateProductItem(Product p)
        {
            SqlCommand cmd;
            if (p.ProductImage != null && p.ProductImage.Length > 0)
            {
                cmd = new SqlCommand("UPDATE Product SET ProductName = productName, CategoryId = @CategoryId, Barcode = @Barcode, NumberInStock = @NumberInStock, PurchasePrice = @PurchasePrice, SalePrice = @SalePrice, Unit = @Unit, ProductImage = @ProductImage, SpecialDiscount = @SpecialDiscount WHERE (ProductId = " + p.Id + ")", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.Parameters.AddWithValue("@Barcode", p.Barcode);
                cmd.Parameters.AddWithValue("@NumberInStock", p.NumberInStock);
                cmd.Parameters.AddWithValue("@PurchasePrice", p.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", p.SalePrice);
                cmd.Parameters.AddWithValue("@Unit", p.Unit);
                cmd.Parameters.AddWithValue("@ProductImage", p.ProductImage);
                cmd.Parameters.AddWithValue("@SpecialDiscount", p.SpecialDiscount);
            }
            else
            {
                cmd = new SqlCommand("UPDATE Product SET ProductName = productName, CategoryId = @CategoryId, Barcode = @Barcode, NumberInStock = @NumberInStock, PurchasePrice = @PurchasePrice, SalePrice = @SalePrice, Unit = @Unit, SpecialDiscount = @SpecialDiscount WHERE (ProductId = " + p.Id + ")", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.Parameters.AddWithValue("@Barcode", p.Barcode);
                cmd.Parameters.AddWithValue("@NumberInStock", p.NumberInStock);
                cmd.Parameters.AddWithValue("@PurchasePrice", p.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", p.SalePrice);
                cmd.Parameters.AddWithValue("@Unit", p.Unit);
                cmd.Parameters.AddWithValue("@SpecialDiscount", p.SpecialDiscount);
            }

            cmd.ExecuteNonQuery();
        }


        // <--  ********************** CRUD methods for Product ************************

        //  ************************** CRUD methods for ProductCategory ************************ -->
        internal string getCategoryNameById(int id)
        {
            string category = "";
            using (SqlCommand cmd = new SqlCommand("SELECT Category FROM ProductCategory WHERE CategoryId=" + id, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            category = reader.GetString(reader.GetOrdinal("Category"));
                        }
                    }
                }
            }
            return category;
        }

        public void AddCategoryItem(ProductCategory pc)
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO ProductCategory (Category, Description) VALUES (@Category, @Description)", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@Category", pc.Category);
            cmd.Parameters.AddWithValue("@Description", pc.Description);
            cmd.ExecuteNonQuery();
        }

        public List<ProductCategory> GetAllCategories()
        {
            List<ProductCategory> CategoryList = new List<ProductCategory>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM ProductCategory", conn))
            {

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                            string category = reader.GetString(reader.GetOrdinal("Category"));
                            string description = reader.GetString(reader.GetOrdinal("Description"));

                            ProductCategory pc = new ProductCategory() { CategoryId = id, Category = category, Description = description };
                            CategoryList.Add(pc);

                        }
                    }
                }
            }
            return CategoryList;
        }

        public void DeleteCategoryItemsById(int Id)
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM ProductCategory WHERE CategoryId = " + Id, conn);
            cmd.ExecuteNonQuery();
        }

        public void UpdateCategoryItems(ProductCategory pc)
        {
            SqlCommand cmd = new SqlCommand("UPDATE ProductCategory SET Category = @Category, Description=@Description WHERE (CategoryId = " + pc.CategoryId + ")", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@Category", pc.Category);
            cmd.Parameters.AddWithValue("@Description", pc.Description);
            cmd.ExecuteNonQuery();
        }


        // <-- ******************* CRUD methods for ProductCategory ************************
        // <-- ******************* CRUD methods for OrderSummary   ************************

        public void AddOrderSummary(OrderSummary orSummary)
        {


            using (SqlCommand cmd = new SqlCommand("INSERT INTO OrderSummary (EmployeeId, CustomerId,DatePurches,TotalPrice,Discount,Tax,Discount,TotalAndTax,PaidMethod,CheckNumber,CreditCardNumber,CardExprDate) VALUES (@EmployeeId, @CustomerId,@DatePurches,@TotalPrice,@Discount,@Tax,@Discount,@TotalAndTax,@PaidMethod,@CheckNumber,@CreditCardNumber,@CardExprDate)"))
            {

                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = conn;

                cmd.Parameters.AddWithValue("@EmployeeId", orSummary.EmployeeId);
                cmd.Parameters.AddWithValue("@CustomerId", orSummary.CustomerId);
                cmd.Parameters.AddWithValue("@DatePurches", orSummary.DatePurches);
                cmd.Parameters.AddWithValue("@TotalPrice", orSummary.TotalPrice);
                cmd.Parameters.AddWithValue("@Discount", orSummary.Discount);
                cmd.Parameters.AddWithValue("@Tax", orSummary.Tax);
                cmd.Parameters.AddWithValue("@TotalAndTax", orSummary.TotalAndTax);
                cmd.Parameters.AddWithValue("@PaidMethod", orSummary.PaidMethod);
                cmd.Parameters.AddWithValue("@CheckNumber", orSummary.CheckNumber);
                cmd.Parameters.AddWithValue("@CardExprDate", orSummary.CardExprDate);
                cmd.Parameters.AddWithValue("@CreditCardNumber", orSummary.CreditCardNumber);
                cmd.Parameters.AddWithValue("@CardExprDate", orSummary.CardExprDate);


                cmd.ExecuteNonQuery();
            }

        }
        public int GetOrderSummaryId(OrderSummary orSummary)
        {



            using (SqlCommand cmd = new SqlCommand("SELECT OrderId FROM OrderSummary WHERE EmployeeId=@EmployeeId  AND DatePurches=@DatePurches AND TotalPrice=@TotalPrice AND CustomerId=@CustomerId  AND PaidMethod=@PaidMethod", conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", orSummary.EmployeeId);
                cmd.Parameters.AddWithValue("@CustomerId", orSummary.CustomerId);
                cmd.Parameters.AddWithValue("@DatePurches", orSummary.DatePurches);
                cmd.Parameters.AddWithValue("@TotalPrice", orSummary.TotalPrice);


                cmd.Parameters.AddWithValue("@PaidMethod", orSummary.PaidMethod);


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("OrderId"));
                            return id;
                        }
                    }
                }

            }
            return 0;
        }
        // <-- ******************* CRUD methods for OrderItems   ************************
        public List<OrderItem> GetAllOrderItemByOrderID(int orderId)
        {
            List<OrderItem> orderIList = new List<OrderItem>();
            SqlCommand cmd = new SqlCommand("SELECT * FROM OrderItem where OrderId=@OrderId ", conn);

            cmd.Parameters.AddWithValue("@OrderId", orderId);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // int orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));
                        string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                        int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));
                        decimal salePricePerUnit = reader.GetDecimal(reader.GetOrdinal("SalePricePerUnit"));
                        int numberOfUnit = reader.GetInt32(reader.GetOrdinal("NumberOfUnit"));




                        OrderItem or = new OrderItem() { OrderId = orderId, ProductName = productName, ProductId = productId, SalePricePerUnit = salePricePerUnit, NumberOfUnit = numberOfUnit };
                        orderIList.Add(or);

                    }

                }
            }
            return orderIList;
        }
        public void AddAllOrderItem(List<OrderItem> Oitems)
        {
            foreach (OrderItem item in Oitems)
            {

                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO OrderItem (OrderId, ProductName, ProductId,SalePricePerUnit) ) values (@OrderId, @ProductName, @ProductId,@SalePricePerUnit,@NumberOfUnit)", conn))
                {
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@OrderId", item.OrderId);
                    cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    cmd.Parameters.AddWithValue("@SalePricePerUnit", item.SalePricePerUnit);

                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}


