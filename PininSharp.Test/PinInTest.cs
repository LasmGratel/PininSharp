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
            Assert.IsTrue(p.Contains("�����ı�", "ceshiwenben"));
            Assert.IsTrue(p.Contains("�����ı�", "ceshiwenbe"));
            Assert.IsTrue(p.Contains("�����ı�", "ceshiwben"));
            Assert.IsTrue(p.Contains("�����ı�", "ce4shi4wb"));
            Assert.IsTrue(!p.Contains("�����ı�", "ce2shi4wb"));
            Assert.IsTrue(p.Contains("�Ͻ�¯", "hejinlu"));
            Assert.IsTrue(p.Contains("ϴ��", "xikuangchang"));
            Assert.IsTrue(p.Contains("����", "liuti"));
            Assert.IsTrue(p.Contains("��20", "hong2"));
            Assert.IsTrue(p.Contains("hong2", "hong2"));
            Assert.IsTrue(!p.Begins("��", "ce4a"));
            Assert.IsTrue(!p.Begins("", "a"));
        }

        [TestMethod]
        public void Daqian()
        {
            var p = new PinIn { Keyboard = Keyboard.Daqian };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("�����ı�", "hk4g4jp61p3"));
            Assert.IsTrue(p.Contains("�����ı�", "hkgjp1"));
            Assert.IsTrue(p.Contains("�a", "vu6"));
            // TODO FIXME
            //Assert.IsTrue(p.Contains("�ʯ", "yj0"));
            //Assert.IsTrue(p.Contains("���|", "j456"));
            Assert.IsTrue(p.Contains("�_�ּ�", "rul3g.3ru84"));
            Assert.IsTrue(p.Contains("�Z", "k6"));
            Assert.IsTrue(p.Contains("�~", "u,4"));
            Assert.IsTrue(p.Contains("��ͬ", "ej/wj/"));
        }

        [TestMethod]
        public void Xiaohe()
        {
            var p = new PinIn { Keyboard = Keyboard.Xiaohe };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("�����ı�", "ceuiwfbf"));
            Assert.IsTrue(p.Contains("�����ı�", "ceuiwf2"));
            Assert.IsTrue(!p.Contains("�����ı�", "ceuiw2"));
            Assert.IsTrue(p.Contains("�Ͻ�¯", "hej"));
            Assert.IsTrue(p.Contains("ϴ��", "xikl4"));
            Assert.IsTrue(p.Contains("����", "ytqq"));
        }

        [TestMethod]
        public void Ziranma()
        {
            var p = new PinIn { Keyboard = Keyboard.Ziranma };
            p.CommitChanges();
            Assert.IsTrue(p.Contains("�����ı�", "ceuiwfbf"));
            Assert.IsTrue(p.Contains("�����ı�", "ceuiwf2"));
            Assert.IsTrue(!p.Contains("�����ı�", "ceuiw2"));
            Assert.IsTrue(p.Contains("�Ͻ�¯", "hej"));
            Assert.IsTrue(p.Contains("ϴ��", "xikd4"));
            Assert.IsTrue(p.Contains("����", "ytqq"));
            Assert.IsTrue(p.Contains("��ȫ", "anqr"));
        }

        [TestMethod]
        public void Tree()
        {
            var tree = new TreeSearcher<int>(SearcherLogic.Contain, new PinIn());
            tree.Put("�����ı�", 1);
            tree.Put("�����з�", 5);
            tree.Put("�����з��ı�", 6);
            tree.Put("�Ͻ�¯", 2);
            tree.Put("ϴ��", 3);
            tree.Put("����", 4);
            tree.Put("��20", 7);
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
            tree.Put("�����ı�", 0);
            tree.Put("��������", 3);
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
                s.Put("�����ı�", 1);
                s.Put("�����з�", 5);
                s.Put("�����з��ı�", 6);
                s.Put("�Ͻ�¯", 2);
                s.Put("ϴ��", 3);
                s.Put("����", 4);
                s.Put("��20", 7);
                s.Put("hong2", 8);
                s.Put("����", 9);
                s.Put("����", 10);
                s.Put("����", 11);
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
            var ch = pi.GetCharacter('Բ');
            var py = ch.Pinyins()[0];
            Assert.IsTrue(PinyinFormat.Number.Format(py).Equals("yuan2"));
            Assert.IsTrue(PinyinFormat.Raw.Format(py).Equals("yuan"));
            Assert.IsTrue(PinyinFormat.Unicode.Format(py).Equals("yu��n"));
            Assert.IsTrue(PinyinFormat.Phonetic.Format(py).Equals("���@"));
            pi.format = PinyinFormat.Phonetic;
            pi.CommitChanges();
            Assert.IsTrue(pi.Format(pi.GetPinyin("le0")).Equals("�B�̨�"));
        }
    }
}