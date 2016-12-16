namespace IBM.WMQ.Nmqi
{
    using System;

    internal interface NmqiXA
    {
        int XA_Close(ManagedHconn Hconn, string name, int rmid, int flags);
        int XA_Commit(ManagedHconn ManagedHconn, MQXid xid, int rmid, int flags);
        int XA_End(ManagedHconn Hconn, MQXid xid, int rmid, int flags);
        int XA_Forget(ManagedHconn Hconn, MQXid xid, int rmid, int flags);
        int XA_Open(ManagedHconn Hconn, string name, int rmid, int flags);
        int XA_Prepare(ManagedHconn Hconn, MQXid xid, int rmid, int flags);
        int XA_Rollback(ManagedHconn Hconn, MQXid xid, int rmid, int flags);
        int XA_Start(ManagedHconn Hconn, MQXid xid, int rmid, int flags);
    }
}

