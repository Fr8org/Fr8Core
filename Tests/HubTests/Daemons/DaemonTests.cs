using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Daemons;
using NUnit.Framework;
using UtilitiesTesting;

namespace HubTests.Daemons
{
    [TestFixture, Ignore]
    public class DaemonTests : BaseTest
    {
        public static void RunDaemonOnce<TDaemon>(TDaemon daemon)
            where TDaemon : Daemon
        {
            try
            {
                typeof(TDaemon).GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(daemon, null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        [Test, Category("Threaded")]
        public void CannotStartDaemonTwice()
        {
            Daemon mockDaemon = new TestDaemon(() => { }, 60);
            Assert.True(mockDaemon.Start());
            Assert.False(mockDaemon.Start());
            mockDaemon.Stop();
        }

        private class TestDaemon : Daemon<TestDaemon>
        {
            private readonly Action _execute;
            private readonly int _timeoutInSeconds;

            public TestDaemon(Action execute, int timeoutInSeconds)
            {
                _execute = execute;
                _timeoutInSeconds = timeoutInSeconds;
            }

            public override int WaitTimeBetweenExecution
            {
                get { return (int)TimeSpan.FromSeconds(_timeoutInSeconds).TotalMilliseconds; }
            }

            protected override void Run()
            {
                _execute();
            }
        }

        private Daemon StartDaemonAndAwaitStartup(Action daemonAction, out Thread workingThread, int timeoutInSeconds = 0)
        {
            object threadLocker = new object();
            Thread workerThread = null;
            TestDaemon mockDaemon = new TestDaemon(
                () =>
                {
                    lock (threadLocker)
                    {
                        workerThread = Thread.CurrentThread;
                        Monitor.Pulse(threadLocker);
                    }
                    daemonAction();
                }, timeoutInSeconds);
            Assert.True(mockDaemon.Start());
            lock (threadLocker)
            {
                while (workerThread == null)
                {
                    Monitor.Wait(threadLocker);
                }
            }

            workingThread = workerThread;
            return mockDaemon;
        }

        [Test, Category("Threaded")]
        public void DaemonGracefullyShutsDown()
        {
            bool hasFinished = false;
            Thread workerThread;
            Daemon mockDaemon = StartDaemonAndAwaitStartup(() =>
            {
                hasFinished = true;
            }, out workerThread);

            mockDaemon.Stop();
            workerThread.Join();

            Assert.True(hasFinished);

            workerThread.Join();
        }

        [Test, Category("Threaded")]
        public void DaemonHandlesExceptionsAndDoesNotCrash()
        {
            object workLock = new object();
            int workNumber = 0;
            bool finished = false;
            Thread workerThread;
            const string testException = "Test exception";
            Daemon mockDaemon = StartDaemonAndAwaitStartup(() =>
            {
                lock (workLock)
                {
                    if (workNumber == 0)
                    {
                        workNumber++;
                        throw new Exception(testException);
                    }
                    finished = true;
                    Monitor.Pulse(workLock);
                }
            }, out workerThread);

            lock (workLock)
                Monitor.Wait(workLock);

            Assert.True(mockDaemon.IsRunning);
            Assert.True(finished);
            Assert.AreEqual(1, mockDaemon.LoggedExceptions.Count);
            Assert.AreEqual(testException, mockDaemon.LoggedExceptions.First().Message);

            mockDaemon.Stop(); //If we don't stop it - the test runner won't ever finished since the thread is still spinning

            try
            {
                workerThread.Abort();
            }
            catch { }
        }
    }
}
