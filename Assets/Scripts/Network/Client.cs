using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network
{
	public class Client : DontDestroySingletonMonoBehaviour<Client>
    {
		private Session session;
		private Reciever reciever = new Reciever ();

		public event Action onAcceptConnect = delegate{};
		public event Action onCloseSession = delegate {};

		public Reciever Reciever
		{
			get {
				return reciever;
			}
		}

		void OnDestroy()
		{
			CloseSession ();
		}

		void OnAcceptConnect()
		{
			onAcceptConnect ();
		}
		void OnCloseSession()
		{
			onCloseSession ();
		}

		public void CreateSession(int port)
		{
			session = new Session (port);
			session.OnAcceptConnect += OnAcceptConnect;
			session.OnCloseSession += OnCloseSession;
			session.OnRecvMessage += Recieve;

			reciever = new Reciever ();
		}

		public void CreateSession(string host, int port)
		{
			session = new Session (host, port);
			session.OnAcceptConnect += OnAcceptConnect;
			session.OnCloseSession += OnCloseSession;
			session.OnRecvMessage += Recieve;

			reciever = new Reciever ();
		}

		public void CloseSession()
		{
			if (session == null) {
				return;
			}
			session.OnAcceptConnect -= OnAcceptConnect;
			session.OnCloseSession -= OnCloseSession;
			session.OnRecvMessage -= Recieve;
			session.Close ();
			session = null;
		}

		public void Send<T>(T msg)
		{
			if (session == null) {
				return;
			}
			session.Send (new Message {
				key = msg.GetType().Name,
				body = JsonUtility.ToJson(msg),
			});
		}

		void Recieve(Network.Message msg)
		{
			Reciever.Recv (msg);
		}

		void Update()
		{
			Reciever.Update ();
		}
    }
}
