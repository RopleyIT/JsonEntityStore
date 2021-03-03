using JsonEntityStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JsonEntityStoreTests
{
    public class TestClass : IID
    {
        public int Id { get; set; }
        public int AnInt { get; set; }
        public double ADouble { get; set; }
        public string AString { get; set; }
        public DateTime ADate { get; set; }
    }

    [TestClass]
    public class StorageTests
    {
        [TestMethod]
        public void CanConstruct()
        {
            string tmpFolder = Path.GetTempPath();
            var storage = new Storage<TestClass>(tmpFolder, false);
            Assert.IsNotNull(storage);
        }

        [TestMethod]
        public void CanConstructZip()
        {
            string tmpFolder = Path.GetTempPath();
            var storage = new Storage<TestClass>(tmpFolder, true);
            Assert.IsNotNull(storage);
        }

        [TestMethod]
        public void CanCreateNew()
        {
            string tmpFolder = Path.GetTempPath();
            var storage = new Storage<TestClass>(tmpFolder, false);
            var tc = storage.New();
            var tc2 = storage.New();
            Assert.IsNotNull(tc);
            Assert.AreEqual(1, tc.Id);
            Assert.AreEqual(2, tc2.Id);
        }

        [TestMethod]
        public void CanCreateNewZip()
        {
            string tmpFolder = Path.GetTempPath();
            var storage = new Storage<TestClass>(tmpFolder, true);
            var tc = storage.New();
            var tc2 = storage.New();
            Assert.IsNotNull(tc);
            Assert.AreEqual(1, tc.Id);
            Assert.AreEqual(2, tc2.Id);
        }

        [TestMethod]
        public void CanSave()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, false);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                Assert.IsTrue(File.Exists(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json")));
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json"));
            }
        }

        [TestMethod]
        public void CanSaveZip()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, true);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                Assert.IsTrue(File.Exists(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json.zip")));
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json.zip"));
            }
        }

        [TestMethod]
        public void CanRead()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, false);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, false);
                Assert.AreEqual(2, storage.Count());
                Assert.AreEqual(2, storage.All().Count);
                Assert.AreEqual(42, storage.All()[0].AnInt);
                Assert.AreEqual(new DateTime(2000, 4, 9, 13, 59, 59),
                    storage.Last().ADate);
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json"));
            }
        }

        [TestMethod]
        public void CanFind()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, false);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, false);
                var foundT = storage.Find(2);
                Assert.IsNotNull(foundT);
                Assert.AreEqual(1.62e-19, foundT.ADouble);
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json"));
            }
        }

        [TestMethod]
        public void CanReadZip()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, true);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, true);
                Assert.AreEqual(2, storage.Count());
                Assert.AreEqual(2, storage.All().Count);
                Assert.AreEqual(42, storage.All()[0].AnInt);
                Assert.AreEqual(new DateTime(2000, 4, 9, 13, 59, 59),
                    storage.Last().ADate);
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json.zip"));
            }
        }

        [TestMethod]
        public void CanFindZip()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, true);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, true);
                var foundT = storage.Find(2);
                Assert.IsNotNull(foundT);
                Assert.AreEqual(1.62e-19, foundT.ADouble);
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json.zip"));
            }
        }

        [TestMethod]
        public void CanDelete()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, false);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, false);
                var foundT = storage.Find(2);
                storage.Delete(foundT);
                Assert.AreEqual(1, storage.All().Count);
                Assert.IsNotNull(storage.Find(1));
                Assert.IsNull(storage.Find(2));
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json"));
            }
        }

        [TestMethod]
        public void CanDeleteZip()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, true);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, true);
                var foundT = storage.Find(2);
                storage.Delete(foundT);
                Assert.AreEqual(1, storage.All().Count);
                Assert.IsNotNull(storage.Find(1));
                Assert.IsNull(storage.Find(2));
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json.zip"));
            }
        }

        [TestMethod]
        public void CanDeleteById()
        {
            string tmpFolder = Path.GetTempPath();
            try
            {
                var storage = new Storage<TestClass>(tmpFolder, false);
                var tc = storage.New();
                tc.AnInt = 42;
                tc.ADate = DateTime.Today;
                tc.ADouble = 3.1415926535;
                tc.AString = "Quick brown fox";
                var tc2 = storage.New();
                tc2.AnInt = 21;
                tc2.ADate = new DateTime(2000, 4, 9, 13, 59, 59);
                tc2.ADouble = 1.62e-19;
                tc2.AString = "Cat in the hat";
                storage.Save();
                storage = new Storage<TestClass>(tmpFolder, false);
                var foundT = storage.Find(2);
                storage.Delete(1);
                Assert.AreEqual(1, storage.All().Count);
                Assert.IsNull(storage.Find(1));
                Assert.IsNotNull(storage.Find(2));
            }
            finally
            {
                File.Delete(Path.Combine
                    (tmpFolder, "JsonEntityStoreTests.TestClass.json"));
            }
        }
    }
}
