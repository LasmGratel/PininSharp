using Microsoft.VisualStudio.TestTools.UnitTesting;
using PininSharp.Searchers;
using PininSharp.Utils;
using System.Collections.Generic;

namespace PininSharp.Test
{
    [TestClass]
    public class PinInTest
    {
        [TestMethod]
        public void Quanpin()
        {
            var p = new PinIn();
            Assert.IsTrue(p.Contains("测试文本", "ceshiwenben"));
            Assert.IsTrue(p.Contains("测试文本", "ceshiwenbe"));
            Assert.IsTrue(p.Contains("测试文本", "ceshiwben"));
            Assert.IsTrue(p.Contains("测试文本", "ce4shi4wb"));
            Assert.IsTrue(!p.Contains("测试文本", "ce2shi4wb"));
            Assert.IsTrue(p.Contains("合金炉", "hejinlu"));
            Assert.IsTrue(p.Contains("洗矿场", "xikuangchang"));
            Assert.IsTrue(p.Contains("流体", "liuti"));
            Assert.IsTrue(p.Contains("轰20", "hong2"));
            Assert.IsTrue(p.Contains("hong2", "hong2"));
            Assert.IsTrue(!p.Begins("测", "ce4a"));
            Assert.IsTrue(!p.Begins("", "a"));
        }

        [TestMethod]
        public void Daqian()
        {
            var p = new PinIn { Keyboard = Keyboard.Daqian };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("测试文本", "hk4g4jp61p3"));
            Assert.IsTrue(p.Contains("测试文本", "hkgjp1"));
            Assert.IsTrue(p.Contains("錫", "vu6"));
            // TODO FIXME
            //Assert.IsTrue(p.Contains("鑽石", "yj0"));
            //Assert.IsTrue(p.Contains("物質", "j456"));
            Assert.IsTrue(p.Contains("腳手架", "rul3g.3ru84"));
            Assert.IsTrue(p.Contains("鵝", "k6"));
            Assert.IsTrue(p.Contains("葉", "u,4"));
            Assert.IsTrue(p.Contains("共同", "ej/wj/"));
        }

        [TestMethod]
        public void Xiaohe()
        {
            var p = new PinIn { Keyboard = Keyboard.Xiaohe };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("测试文本", "ceuiwfbf"));
            Assert.IsTrue(p.Contains("测试文本", "ceuiwf2"));
            Assert.IsTrue(!p.Contains("测试文本", "ceuiw2"));
            Assert.IsTrue(p.Contains("合金炉", "hej"));
            Assert.IsTrue(p.Contains("洗矿场", "xikl4"));
            Assert.IsTrue(p.Contains("月球", "ytqq"));
        }

        [TestMethod]
        public void Ziranma()
        {
            var p = new PinIn { Keyboard = Keyboard.Ziranma };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("测试文本", "ceuiwfbf"));
            Assert.IsTrue(p.Contains("测试文本", "ceuiwf2"));
            Assert.IsTrue(!p.Contains("测试文本", "ceuiw2"));
            Assert.IsTrue(p.Contains("合金炉", "hej"));
            Assert.IsTrue(p.Contains("洗矿场", "xikd4"));
            Assert.IsTrue(p.Contains("月球", "ytqq"));
            Assert.IsTrue(p.Contains("安全", "anqr"));
        }

        [TestMethod]
        public void Tree()
        {
            var tree = new TreeSearcher<int>(SearcherLogic.Contain, new PinIn());
            tree.Put("测试文本", 1);
            tree.Put("测试切分", 5);
            tree.Put("测试切分文本", 6);
            tree.Put("合金炉", 2);
            tree.Put("洗矿场", 3);
            tree.Put("流体", 4);
            tree.Put("轰20", 7);
            tree.Put("hong2", 8);

            List<int> s;
            s = tree.Search("ceshiwenben");
            Assert.IsTrue(s.Count == 1 && s.Contains(1));
            s = tree.Search("ceshiwenbe");
            Assert.IsTrue(s.Count == 1 && s.Contains(1));
            s = tree.Search("ceshiwben");
            Assert.IsTrue(s.Count == 1 && s.Contains(1));
            s = tree.Search("ce4shi4wb");
            Assert.IsTrue(s.Count == 1 && s.Contains(1));
            s = tree.Search("ce2shi4wb");
            Assert.IsTrue(s.Count == 0);
            s = tree.Search("hejinlu");
            Assert.IsTrue(s.Count == 1 && s.Contains(2));
            s = tree.Search("xikuangchang");
            Assert.IsTrue(s.Count == 1 && s.Contains(3));
            s = tree.Search("liuti");
            Assert.IsTrue(s.Count == 1 && s.Contains(4));
            s = tree.Search("ceshi");
            Assert.IsTrue(s.Count == 3 && s.Contains(1) && s.Contains(5));
            s = tree.Search("ceshiqiefen");
            Assert.IsTrue(s.Count == 2 && s.Contains(5));
            s = tree.Search("ceshiqiefenw");
            Assert.IsTrue(s.Count == 1 && s.Contains(6));
            s = tree.Search("hong2");
            Assert.IsTrue(s.Contains(7) && s.Contains(8));
        }

        [TestMethod]
        public void Context()
        {
            var p = new PinIn();
            var tree = new TreeSearcher<int>(SearcherLogic.Contain, p);
            tree.Put("测试文本", 0);
            tree.Put("测试文字", 3);
            List<int> s;
            s = tree.Search("ce4shi4wb");
            Assert.IsTrue(s.Count == 1 && s.Contains(0));
            s = tree.Search("ce4shw");
            Assert.IsTrue(s.Count == 2);
            s = tree.Search("ce4sw");
            Assert.IsTrue(s.Count == 2);
            s = tree.Search("ce4siw");
            Assert.IsTrue(s.Count == 0);
            p.fSh2S = true;
            p.CommitChanges();
            s = tree.Search("ce4siw");
            Assert.IsTrue(s.Count == 2);
            p.fSh2S = false;
            p.Keyboard = Keyboard.Daqian;
            p.CommitChanges();
            s = tree.Search("hk4g4jp61p3");
            Assert.IsTrue(s.Count == 1);
            s = tree.Search("ce4shi4wb");
            Assert.IsTrue(s.Count == 0);
        }

        [TestMethod]
        public void Full()
        {
            var ss = new List<ISearcher<int>>
            {
                new TreeSearcher<int>(SearcherLogic.Equal, new PinIn()),
                new SimpleSearcher<int>(SearcherLogic.Equal, new PinIn())
            };
            foreach (var s in ss)
            {
                s.Put("测试文本", 1);
                s.Put("测试切分", 5);
                s.Put("测试切分文本", 6);
                s.Put("合金炉", 2);
                s.Put("洗矿场", 3);
                s.Put("流体", 4);
                s.Put("轰20", 7);
                s.Put("hong2", 8);
                s.Put("月球", 9);
                s.Put("汉化", 10);
                s.Put("喊话", 11);
                List<int> list;
                list = s.Search("hong2");
                Assert.IsTrue(list.Count == 1 && list.Contains(8));
                list = s.Search("hong20");
                Assert.IsTrue(list.Count == 1 && list.Contains(7));
                list = s.Search("ceshqf");
                Assert.IsTrue(list.Count == 1 && list.Contains(5));
                list = s.Search("ceshqfw");
                Assert.IsTrue(list.Count == 0);
                list = s.Search("hh");
                Assert.IsTrue(list.Count == 2 && list.Contains(10) && list.Contains(11));
                list = s.Search("hhu");
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void Format()
        {
            var pi = new PinIn();
            var ch = pi.GetCharacter('圆');
            var py = ch.Pinyins()[0];
            Assert.IsTrue(PinyinFormat.Number.Format(py).Equals("yuan2"));
            Assert.IsTrue(PinyinFormat.Raw.Format(py).Equals("yuan"));
            Assert.IsTrue(PinyinFormat.Unicode.Format(py).Equals("yuán"));
            Assert.IsTrue(PinyinFormat.Phonetic.Format(py).Equals("ㄩㄢˊ"));
            pi.format = PinyinFormat.Phonetic;
            pi.CommitChanges();
            Assert.IsTrue(pi.Format(pi.GetPinyin("le0")).Equals("˙ㄌㄜ"));
        }
    }
}