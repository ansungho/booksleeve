﻿using BookSleeve;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class Constraints
    {
        [Test]
        public void TestManualIncr()
        {
            using (var conn = Config.GetUnsecuredConnection())
            {
                conn.Keys.Remove(0, "foo");
                Assert.AreEqual(1, conn.Wait(ManualIncr(conn, 0, "foo")));
                Assert.AreEqual(2, conn.Wait(ManualIncr(conn, 0, "foo")));
                Assert.AreEqual(2, conn.Wait(conn.Strings.GetInt64(0, "foo")));
            }
            
        }

        public async Task<long?> ManualIncr(RedisConnection connection, int db, string key)
        {
            var oldVal = await connection.Strings.GetInt64(db, key);
            var newVal = (oldVal ?? 0) + 1;
            using (var tran = connection.CreateTransaction())
            { // check hasn't changed
                tran.AddCondition(Condition.KeyEquals(db, key, oldVal));
                tran.Strings.Set(db, key, newVal);
                if (!await tran.Execute()) return null; // aborted
                return newVal;
            }    
        }
    }
}