
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Collections.Generic;
using System.Linq;

namespace StoreTestProject
{

    [TestClass()]
    public class StoreDiffTest
    {
        string GetDifferences(string s1, string s2)
        {
            string[] ss1 = s1.Split(new[] { ' ', ',' });
            int[] a1 = ss1.Select(s => Int32.Parse(s)).ToArray<int>();

            string[] ss2 = s2.Split(new[] { ' ', ',' });
            int[] a2 = ss2.Select(s => Int32.Parse(s)).ToArray<int>();

            int[] diff = new int[a1.Length];

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    diff[i] = a2[i];
            }

            return string.Join(",", diff);
        }

        [TestMethod()]
        public void TestGetDifferences()
        {
            string actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "1,2,3,4,5,6,7,8,9,10");
            Assert.AreEqual("0,0,0,0,0,0,0,0,0,0", actual);

            actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "4,1,2,3,5,6,7,8,9,10");
            Assert.AreEqual("4,1,2,3,0,0,0,0,0,0", actual);

            actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "2,3,4,5,6,1,7,8,9,10");
            Assert.AreEqual("2,3,4,5,6,1,0,0,0,0", actual);

            actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "2,1,3,4,5,6,7,8,9,10");
            Assert.AreEqual("2,1,0,0,0,0,0,0,0,0", actual);

            actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "1,2,3,4,5,6,7,8,10,9");
            Assert.AreEqual("0,0,0,0,0,0,0,0,10,9", actual);

            actual = GetDifferences("1,2,3,4,5,6,7,8,9,10", "1,3,2,4,5,6,7,9,8,10");
            Assert.AreEqual("0,3,2,0,0,0,0,9,8,0", actual);

            actual = GetDifferences("1,2,3,4,6,7,5,8,9,10", "1,2,3,4,5,6,7,8,9,10");
            Assert.AreEqual("0,0,0,0,5,6,7,0,0,0", actual);
        }
    }

    /// <summary>
    ///Это класс теста для StoreOrderTest, в котором должны
    ///находиться все модульные тесты StoreOrderTest
    ///</summary>
    [TestClass()]
    public class StoreOrderTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private string ApplyStoresOrderTest(string strStores, string strChangedStores, bool fAtBottomOfPage, int currentPage, int pageSize)
        {
            string[] stores = strStores.Split(new[] { ' ', ',' });
            IEnumerable<int> stores_id = stores.Select(s => Int32.Parse(s));

            string[] new_stores = strChangedStores.Split(new[] { ' ', ',' });
            IEnumerable<int> new_stores_id = new_stores.Select(s => Int32.Parse(s));


            int nStart = (currentPage - 1) * pageSize;
            if (fAtBottomOfPage)
            {
                //Если строки переносились в конец страницы и первые склады не попали на текущую страницу
                //или количество складов в списке меньше размера страницы, то произойдет корректировка начала размещения
                //if (new_stores_id.Count() > pageSize)
                    nStart += (pageSize - new_stores_id.Count());
                    if (nStart < 0) nStart = 0;
            }

            //строки на последнюю страницу целиком они не влезают
            if (nStart + new_stores_id.Count() > stores_id.Count())
                nStart = stores_id.Count() - new_stores_id.Count();

            int[] first_last = { nStart+1, nStart + new_stores_id.Count() };
            int fl_i = 0;

            try
            {
                int count = stores_id.Count();
                int[] new_stores_order = new int[count];
                int new_index = 0;

                /*
                int i = 0;

                //Копируем элементы до текущей страницы
                while (new_index < nStart && i < count)
                {
                    int store_id = stores_id.ElementAt(i++);
                    if (!new_stores_id.Contains<int>(store_id))
                        new_stores_order[new_index++] = store_id;
                }

                //Копируем текущую страницу
                foreach (int id in new_stores_id)
                    new_stores_order[new_index++] = id;

                //Копируем все остальные элементы
                while (i < count)
                {
                    int store_id = stores_id.ElementAt(i++);
                    if (!new_stores_id.Contains<int>(store_id))
                        new_stores_order[new_index++] = store_id;
                }
                */


            int i = 0;
            while (i < count && new_index<count)
            {
                if (new_index < nStart)
                {
                    int store_id = stores_id.ElementAt(i++);
                    if (!new_stores_id.Contains<int>(store_id))
                        new_stores_order[new_index++] = store_id;
                    else
                    {
                        bool f = fl_i == 0 ? first_last[fl_i] > i : first_last[fl_i] < i;
                        if(f) first_last[fl_i]=i;
                    }
                }

                //Условие проверяется снова потому - что new_index++ и i++, следующей итерации уже может и не быть
                if (new_index >= nStart)
                {
                    //Копируем текущую страницу
                    foreach (int id in new_stores_id)
                        new_stores_order[new_index++] = id;

                    nStart = count+1;

                    fl_i = 1;
                }
            }


            return string.Join(",", first_last) + "-" + string.Join(",", new_stores_order);
            }
            catch (Exception)
            {
            }

            return null;
        }

        /// <summary>
        ///Тест для ApplyStoresOrderTest
        ///</summary>
        [TestMethod()]
        public void ApplyStoresOrderTestTest()
        {
            string strStores = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            string strChangedStores = "1,2,3,4"; // TODO: инициализация подходящего значения
            bool fAtBottomOfPage = false; // TODO: инициализация подходящего значения
            int currentPage = 2; // TODO: инициализация подходящего значения
            int pageSize = 4; // TODO: инициализация подходящего значения
            string expected = "1,8-5,6,7,8,1,2,3,4,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            string actual;
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Проверьте правильность этого метода теста.");

            currentPage = 1;
            expected = "1,4-1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 3;
            expected = "1,12-5,6,7,8,9,10,11,12,1,2,3,4,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 4;
            expected = "1,16-5,6,7,8,9,10,11,12,13,14,15,16,1,2,3,4,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            strChangedStores = "1,2";
            expected = "1,14-3,4,5,6,7,8,9,10,11,12,13,14,1,2,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = true;
            expected = "1,16-3,4,5,6,7,8,9,10,11,12,13,14,15,16,1,2,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 5;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,20-6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,1,2,3,4,5"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = false;
            expected = "1,20-6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,1,2,3,4,5"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 2;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,8-6,7,8,1,2,3,4,5,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = false;
            expected = "1,9-6,7,8,9,1,2,3,4,5,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 2;
            strChangedStores = "1,2,3,4,5,6,7,8,9,10";
            fAtBottomOfPage = true;
            expected = "1,10-1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = false;
            expected = "1,14-11,12,13,14,1,2,3,4,5,6,7,8,9,10,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);


            currentPage = 4;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,16-6,7,8,9,10,11,12,13,14,15,16,1,2,3,4,5,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = false;
            expected = "1,17-6,7,8,9,10,11,12,13,14,15,16,17,1,2,3,4,5,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 1;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,5-1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            currentPage = 1;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = false;
            expected = "1,5-1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            pageSize = 1;
            currentPage = 3;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = false;
            expected = "1,7-6,7,1,2,3,4,5,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);


            pageSize = 1;
            currentPage = 3;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,5-1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);


            pageSize = 4;
            currentPage = 1;
            strChangedStores = "8";
            fAtBottomOfPage = false;
            expected = "1,8-8,1,2,3,4,5,6,7,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            pageSize = 4;
            currentPage = 1;
            strChangedStores = "8";
            fAtBottomOfPage = true;
            expected = "4,8-1,2,3,8,4,5,6,7,9,10,11,12,13,14,15,16,17,18,19,20"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);


            pageSize = 40;
            currentPage = 3;
            strChangedStores = "1,2,3,4,5";
            fAtBottomOfPage = true;
            expected = "1,20-6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,1,2,3,4,5"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            fAtBottomOfPage = false;
            expected = "1,20-6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,1,2,3,4,5"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            pageSize = 6;
            currentPage = 4;
            strChangedStores = "8";
            fAtBottomOfPage = true;
            expected = "8,20-1,2,3,4,5,6,7,9,10,11,12,13,14,15,16,17,18,19,20,8"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

            strStores = "1,2,3,4,5,6,7,8,9,10,11";
            pageSize = 11;
            currentPage = 1;
            strChangedStores = "6";
            fAtBottomOfPage = true;
            expected = "6,11-1,2,3,4,5,7,8,9,10,11,6"; // TODO: инициализация подходящего значения
            actual = ApplyStoresOrderTest(strStores, strChangedStores, fAtBottomOfPage, currentPage, pageSize);
            Assert.AreEqual(expected, actual);

        }
    }
}
