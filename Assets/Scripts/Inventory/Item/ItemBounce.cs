using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Inventory.Item
{
    public class ItemBounce : MonoBehaviour
    {
        [SerializeField] private Transform itemTrans;
        [SerializeField] private BoxCollider2D coll;

        public float throwAcceleration; //抛出物体的加速度
        private bool _landed; //判断物体是否落地
        private float _landSpeed;
        private float _throwDistance;
        private Vector2 _throwDirection;
        private Vector3 _targetPos;

        private void Update()
        {
            Bounce();
        }

        public void InitBounceItem(Vector3 target, Vector2 dir)
        {
            //刚扔出去时关闭碰撞体，防止又被玩家捡回来
            coll.enabled = false;
            _throwDirection = dir;
            _targetPos = target;

            _throwDistance = Vector3.Distance(target, transform.position);
            throwAcceleration *= _throwDistance; //扔出的距离越远速度越快
            /*
             * 水平和垂直时间是相等的，所以可以根据水平飞行的参数求出竖直方向的飞行速度
             * 水平飞行距离：Distance: Vector3.Distance(target, transform.position)
             * 水平飞行时间：time: Distance / throwAcceleration
             * 竖直方向的速度：1.5f / time
             */
            // _landSpeed = 1.5f / (throwDistance / throwAcceleration);
            _landSpeed = 1.5f * throwAcceleration / _throwDistance; //简化

            //物体在玩家头上的距离
            itemTrans.position += Vector3.up * 1.5f;
        }

        /// <summary>
        /// 影子才是改物体的本体，子物体的图片逐渐向影子靠拢，靠拢后说明该物体落地了
        /// </summary>
        private void Bounce()
        {
            //only need uniform descent
            if (_throwDistance is 0)
            {
                if(itemTrans.localPosition.magnitude > 0.1f)
                {
                    itemTrans.position += Vector3.down * 4 * Time.deltaTime;
                    return;
                }
                coll.enabled = true;
                Destroy(this);
                return;
            }
            
            _landed = itemTrans.position.y <= transform.position.y;

            if (Vector3.Distance(_targetPos, transform.position) > 0.1f)
                //水平方向根据玩家的的扔出速度改变位置
                transform.position += (Vector3)_throwDirection * (throwAcceleration * Time.deltaTime);

            if (!_landed)
            {
                //竖直方向根据速度改变位置
                itemTrans.position += Vector3.down * (_landSpeed * Time.deltaTime);
                return;
            }

            coll.enabled = true;
            //落地后删除脚本节省性能
            Destroy(this);
        }
    }
}