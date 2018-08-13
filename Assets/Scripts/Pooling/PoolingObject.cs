using UnityEngine;

namespace pooling
{
	//Abstract class to be inherited to be able to use Pooling.cs
	public abstract class PoolingObject : MonoBehaviour, IPooling
    {
		public virtual string objectName{ get { return ""; } }
		public bool isUsing { get; set; }

        //Gets called whenever is collected
        public virtual void OnCollect()
        {
			isUsing = true;
            gameObject.SetActive(true);
        }

		//Gets called whenever is released
        public virtual void OnRelease()
        {
			isUsing = false;
            gameObject.SetActive(false);
        }
    }
}