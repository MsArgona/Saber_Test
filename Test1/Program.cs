using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Linq;
using System.Reflection;

class Test
{
    public static void Main()
    {
        const string PATH = @"L:\TestSaber\test.csv";
        int count = 5; // count of nodes

        //Creating a List
        ListRand listRand = new ListRand();
        listRand.Count = count;

        //Filling the List with Nodes, where "i" is the Data
        for (int i = 0; i < count; i++)
            AddNodeTo(listRand, i.ToString());

        //Rand field initialization
        ListNode next = listRand.Head;
        while (next != null)
        {
            next.Rand = GetRandomNodeFrom(listRand);
            next = next.Next;
        }

        PrintList(listRand.Head);

        //Serialize
        FileStream s = File.Create(PATH);
        using (s)
        {
            listRand.Serialize(s);
        }

        //Deserialize
        s = File.OpenRead(PATH);
        ListRand newListRand = new ListRand();
        using (s)
        {
            newListRand.Deserialize(s);
        }

        Console.WriteLine("\nDeserialize");
        PrintList(newListRand.Head);
    }

    class ListNode
    {
        public ListNode Prev;
        public ListNode Next;
        public ListNode Rand; // произвольный элемент внутри списка
        public string Data;
    }

    class ListRand
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        public void Serialize(FileStream s)
        {
            //Node, Index, Rand
            Dictionary<ListNode, Tuple<int, ListNode>> tmpDictionary = new Dictionary<ListNode, Tuple<int, ListNode>>();

            //Fill Dictionary
            ListNode next = Head;
            int index = 0;
            while (next != null)
            {
                //Node, Index, Rand
                tmpDictionary.Add(next, new(index, next.Rand));
                next = next.Next;
                index++;
            }

            //Write Rand id and Data from the Dictionary
            next = Head;
            index = 0;
            while (next != null)
            {
                //Rand node index, Data
                AddText(s, (tmpDictionary[tmpDictionary[next].Item2].Item1.ToString() + "," + next.Data.ToString() + "\n"));
                next = next.Next;
                index++;
            }
        }

        public void Deserialize(FileStream s)
        {
            //<Index in the list, Tuple<Rand index, Rand ListNode>>
            Dictionary<int, Tuple<int, ListNode>> nodeData = new Dictionary<int, Tuple<int, ListNode>>();

            int index = 0;

            //Read data from csv file, fill Dictionary and fill list with nodes data without rand nodes
            using (s)
            {
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                int readLen;
                while ((readLen = s.Read(b, 0, b.Length)) > 0)
                {
                    string[] tmpStr = (temp.GetString(b, 0, readLen - 1)).Split(new char[] { '\n', '\r', ',' });

                    for (int i = 0; i < tmpStr.Length; i += 2)
                    {
                        //delete all newlines
                        tmpStr[i] = tmpStr[i].Replace("\n", "").Replace("\r", "");
                        tmpStr[i + 1] = tmpStr[i + 1].Replace("\n", "").Replace("\r", "");

                        //Fill list with nodes data without rand nodes
                        AddNodeTo(this, tmpStr[i + 1]);

                        //Fill Dictionary
                        nodeData.Add(index, new(Int32.Parse(tmpStr[i]), Tail));

                        //Console.WriteLine(index + ", " + nodeData[index]);
                        index++;
                    }
                }
            }

            // Fill list with rand node data from Dictionary
            ListNode next = Head;
            index = 0;
            while (next != null)
            {
                int randNodeIndex = nodeData[index].Item1;
                next.Rand = nodeData[randNodeIndex].Item2;

                next = next.Next;
                index++;
            }
        }
    }

    private static int GetNodeNumIn(ListRand listRand, ListNode desiredNode)
    {
        int index = 0;

        ListNode next = listRand.Head;
        while (next != null)
        {
            if (next == desiredNode)
                return index;

            index++;
            next = next.Next;
        }

        return -1;
    }

    private static ListNode? GetRandomNodeFrom(ListRand listRand)
    {
        Random rnd = new Random();
        int randIndex = rnd.Next(listRand.Count); //0..4
        int curIndex = 0;

        ListNode next = listRand.Head;
        while (next != null)
        {
            if (curIndex == randIndex)
                return next;

            curIndex++;
            next = next.Next;
        }

        return null;
    }

    private static void PrintList(ListNode startNode)
    {
        Console.WriteLine("\nData\tRand.Data\n");

        ListNode next = startNode;
        while (next != null)
        {
            Console.Write($"{next.Data}\t{next.Rand.Data}\n");
            next = next.Next;
        }
    }

    private static void AddNodeTo(ListRand listRand, string data)
    {
        ListNode newNode = new ListNode();
        newNode.Data = data;

        if (listRand.Head == null)
            listRand.Head = newNode;
        else
        {
            listRand.Tail.Next = newNode;
            newNode.Prev = listRand.Tail;
        }
        listRand.Tail = newNode;
    }

    private static void AddText(FileStream s, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        s.Write(info, 0, info.Length);
    }
}