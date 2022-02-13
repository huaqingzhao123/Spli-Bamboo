using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
/// <summary>
/// 切割图片
/// </summary>
namespace hbj
{
    public class CutSprite : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private int _width;
        [SerializeField]
        private int _heigth;
        [SerializeField]
        private Vector2 _lossyScale;
        /// <summary>
        /// 切角默认先45度
        /// </summary>
        [SerializeField,Header("竹子横截面切角")]
        private float _cutAngle = 45;
        /// <summary>
        /// 方向
        /// </summary>
        private Vector2 _dirBegin;
        private Rigidbody2D _rigidbody2D;
        private Texture2D _texture2D;
        private Color[] _pixels;
        Vector3 _bottomTriangleP1;
        Vector3 _bottomTriangleP2;
        Vector3 _bottomTriangleP3;
        Vector3 _bottomTriangleP4;
        Vector3 _topTriangleP1;
        //右下点
        Vector3 _topTriangleP2;
        //左顶部点
        Vector3 _topTriangleP3;
        //右边顶部点
        Vector3 _topTriangleP4;
        //竹子掉落的最终角度
        [Header("竹子掉落的最终角度")]
        public float _dropAngle = -80;
        public float _timeScale = 1f;
        [Header("竹子掉落的总时间")]
        public float _dropTime = 1.5f;
        [Header("竹子掉落的重力系数")]
        public float _gravity = 0.2f;
        [Header("竹子掉落的加速曲线")]
        public Ease _dropEase = Ease.Linear;
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            SetSize();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
                _rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody2D.gravityScale = 0;
            Debug.LogError("像素的数量:" + _pixels.Length);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                _dirBegin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CutBomboo(_dirBegin, null);
            }
#endif
        }


        /// <summary>
        /// 切竹子
        /// </summary>
        public void CutBomboo(Vector2 dirBegin, Action callback)
        {
            //此处3804为竹子在场景中大小，实际图片大小也为3804只不过在unity压缩后变为两千多
            var spriteRatio = (float)3804*transform.lossyScale.y / _heigth;
            //根据竹子宽度和切角度数求出竖直方向上的像素高度,生成图片判断位置用
            var ratio = Mathf.Tan(_cutAngle / 180 * 3.14f);
            //纹理图上的裁剪区域高度 
            var verticalLength =_width * ratio;
            //映射到游戏内的真实高度
            var realGameLength = verticalLength / 100 * spriteRatio;
            //被切点处在切面的正中心，所以切面顶部位置应该加上切面高度一半
            _dirBegin = dirBegin + new Vector2(0, realGameLength / 2);
            //下半部分,由切面顶部到原点的距离算出纹理图上切点到半径的距离(带正负)，加上半径就为切面顶边到纹理图底部的实际纹理距离即下半部分距离
            //下半部分理论尺寸
            var bottomHeight = (_dirBegin.y - transform.position.y) * 100 / spriteRatio + _heigth / 2;
            //上半部分理论尺寸
            var topHeight = _heigth - bottomHeight + verticalLength;
            //计算各自超出高度
            //这两种情况都分别在两端不足r的情况,即0.5r到r之间，即在两边界往内延申0.5r范围中砍竹子,此时裁切区域的坐标需要特殊处理
            //顶部在0.5r到r时,自身裁剪三角形坐标计算没问题，但是底部的需要在正常方式下往上延申common1
            int common1 = 0;
            //底部在0.5r到r时,自身裁剪三角形坐标计算没问题，但是顶部的需要在正常方式下往下延申common2
            int common2 = 0;
            if (bottomHeight > _heigth)
            {
                common1 = (int)(bottomHeight - _heigth);
            }
            else if (topHeight > _heigth)
            {
                common2 = (int)(topHeight - _heigth);
            }
            bottomHeight = Mathf.Clamp(bottomHeight, verticalLength / 2, _heigth);
            topHeight = Mathf.Clamp(topHeight, verticalLength / 2, _heigth);
            //生成两张新的texture
            var texture1 = new Texture2D(_width, (int)bottomHeight, TextureFormat.RGBA32, false);
            var array1 = new Color[(int)bottomHeight * _width];
            //画下半部分竹子纹理
            for (int i = 0; i < (int)bottomHeight; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    array1[i * _width + j] = _pixels[i * _width + j];
                }
            }
            var array2 = new Color[(_heigth - (int)(bottomHeight - verticalLength)) * _width];
            var texture2 = new Texture2D(_width, (int)topHeight, TextureFormat.RGBA32, false);
            //第二张图的底部坐标
            var texture2Bottom = (int)bottomHeight - (int)verticalLength;
            if (texture2Bottom < 0) texture2Bottom = 0;
            if (common1 > 0) texture2Bottom =(int)(_heigth - verticalLength + common1);
            //画上半部分竹子纹理
            for (int i = texture2Bottom; i < _heigth; i++)
            {
                try
                {
                    for (int j = 0; j < _width; j++)
                    {
                        try
                        {
                            array2[(i - texture2Bottom) * _width + j] = _pixels[i * _width + j];
                        }
                        catch (Exception e)
                        {
                            Debug.LogErrorFormat("报错:{0},i:{1},j:{2}", e.Message, i, j);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    break;
                }
            }
            Debug.LogErrorFormat("下半部分高度:{0},上半部分高度:{1},切角直角边长度真实:{2},真实高度:{3}", bottomHeight, topHeight, verticalLength, realGameLength);
            //var sprite1 = Sprite.Create(_texture2D,new Rect(0,0,_texture2D.width, bottomHeight),new Vector2(0.5f,0.5f));
            //var sprite2 = Sprite.Create(_texture2D, new Rect(0, bottomHeight- verticalLength, _texture2D.width, topHeight), new Vector2(0.5f, 0.5f));
            var sprite1 = Sprite.Create(texture1, new Rect(0, 0, texture1.width, texture1.height), new Vector2(0.5f, 0.5f));
            var sprite2 = Sprite.Create(texture2, new Rect(0, 0, texture2.width, texture2.height), new Vector2(0, 0));
            var obj1 = Instantiate(gameObject);
            var obj2 = Instantiate(gameObject);
            obj1.name = "obj1";
            obj2.name = "obj2";
            obj1.transform.localPosition = transform.localPosition;
            obj2.transform.localPosition = transform.localPosition;
            var renderer1 = obj1.GetComponent<SpriteRenderer>();
            var renderer2 = obj2.GetComponent<SpriteRenderer>();
            renderer1.sprite = sprite1;
            renderer2.sprite = sprite2;
            obj1.transform.localScale = Vector3.one * spriteRatio;
            obj2.transform.localScale = Vector3.one * spriteRatio;
            obj1.transform.SetParent(transform.parent);
            obj2.transform.SetParent(transform.parent);
            var size1 = renderer1.sprite.bounds.size;
            var size2 = renderer2.sprite.bounds.size;
            var obj1Dis = (size1.y / 2 * spriteRatio/transform.lossyScale.y - _spriteRenderer.sprite.bounds.size.y / 2)*transform.lossyScale.y;
            var obj2Dis = (_spriteRenderer.sprite.bounds.size.y / 2 - size2.y* spriteRatio / transform.lossyScale.y)*transform.lossyScale.y;
            //obj1.transform.position = transform.position + new Vector3(0, -(_spriteRenderer.sprite.bounds.size.y / 2 - size1.y / 2 * spriteRatio));
            obj1.transform.position = transform.position + new Vector3(0, obj1Dis);
            //obj2.transform.position = transform.position + new Vector3(-size2.x / 2 * spriteRatio, _spriteRenderer.sprite.bounds.size.y / 2 - size2.y * spriteRatio);
            obj2.transform.position = transform.position + new Vector3(-size2.x / 2 * spriteRatio, obj2Dis);
            Destroy(obj1.GetComponent<CutSprite>());
            Destroy(obj2.GetComponent<CutSprite>());
            //划分切割处有像素的三角形区域
            //左下点,从底部竹子最高点开始，向下减出一个裁切三角形面
            _bottomTriangleP1 = new Vector3(-texture1.width / 2, texture1.height / 2 - verticalLength);
            //右下点
            _bottomTriangleP2 = new Vector3(texture1.width / 2, texture1.height / 2 - verticalLength);
            //左顶部点
            _bottomTriangleP3 = new Vector3(-texture1.width / 2, texture1.height / 2);
            //右边顶部点
            _bottomTriangleP4 = new Vector3(texture1.width / 2, texture1.height / 2);
            //多一步判断点在竹子内再进行判断
            //左下点,从顶部竹子最高点开始，向下减出一个裁切三角形面
            _topTriangleP1 = new Vector3(-texture2.width / 2, -texture2.height / 2);
            //右下点
            _topTriangleP2 = new Vector3(texture2.width / 2, -texture2.height / 2);
            //左顶部点
            _topTriangleP3 = new Vector3(-texture2.width / 2, -texture2.height / 2 + verticalLength);
            //右边顶部点
            _topTriangleP4 = new Vector3(texture2.width / 2, -texture2.height / 2 + verticalLength);
            //顶部在r范围，底部超出h范围,特殊处理底部裁剪坐标,往上延申common1
            if (common1 > 0)
            {
                //左下点,从底部竹子最高点开始，向下减出一个裁切三角形面
                var topP =  texture1.height / 2 + common1;
                _bottomTriangleP1 = new Vector3(-texture1.width / 2, topP - verticalLength);
                //右下点
                _bottomTriangleP2 = new Vector3(texture1.width / 2, topP - verticalLength);
                //左顶部点
                _bottomTriangleP3 = new Vector3(-texture1.width / 2, topP);
                //右边顶部点
                _bottomTriangleP4 = new Vector3(texture1.width / 2, topP);
            }
            //下半部分在r范围,上部分超出h范围,,特殊处理顶部部裁剪坐标,往下延申common2
            else if (common2>0)
            {
                var bottomP= -texture2.height / 2 - common2;
                //多一步判断点在竹子内再进行判断
                //左下点,从顶部竹子最高点开始，向下减出一个裁切三角形面
                _topTriangleP1 = new Vector3(-texture2.width / 2, bottomP);
                //右下点
                _topTriangleP2 = new Vector3(texture2.width / 2, bottomP);
                //左顶部点
                _topTriangleP3 = new Vector3(-texture2.width / 2, bottomP + verticalLength);
                //右边顶部点
                _topTriangleP4 = new Vector3(texture2.width / 2, bottomP + verticalLength);
            }
#if UNITY_EDITOR
            StopAllCoroutines();
            //将纹理图的切割区域映射到屏幕上
            var line1 = _bottomTriangleP1 / 100 * spriteRatio + obj1.transform.position;
            var line2 = _bottomTriangleP2 / 100 * spriteRatio + obj1.transform.position;
            var line3 = _bottomTriangleP3 / 100 * spriteRatio + obj1.transform.position;
            StartCoroutine(DrawLine(line1 * 100, line2 * 100, line3 * 100, Color.red));
            var line21 = _topTriangleP1 / 100 * spriteRatio + obj2.transform.position + new Vector3(3.52f, texture2.height / 200 * spriteRatio);
            var line22 = _topTriangleP2 / 100 * spriteRatio + obj2.transform.position + new Vector3(3.52f, texture2.height / 200 * spriteRatio);
            var line23 = _topTriangleP3 / 100 * spriteRatio + obj2.transform.position + new Vector3(3.52f, texture2.height / 200 * spriteRatio);
            StartCoroutine(DrawLine(line21 * 100, line22 * 100, line23 * 100, Color.yellow));
#endif
            int index1 = 0;
            int index2 = 0;
            //填充空白像素，从切割的位置开始遍历所有行,此处用像素高度对纹理进行处理
            for (int i = 0; i < (int)verticalLength; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    var pos1 = _bottomTriangleP4 + new Vector3(-j, -i);
                    //先判断坐标是否在纹理范围内
                    if ((common1>0||common2>0)&&!MathfUtil.IsVector2PointInMatrix(new Vector3(-texture1.width / 2, -texture1.height / 2), new Vector3(texture1.width / 2, -texture1.height / 2), new Vector3(texture1.width / 2, texture1.height / 2), new Vector3(-texture1.width / 2, texture1.height / 2), pos1)) continue;
                    //不在像素区域内进行透明处理                  
                    if (!MathfUtil.Vector2IsInTriangle(_bottomTriangleP1, _bottomTriangleP2, _bottomTriangleP3, pos1)
                        && array1.Length > index1 && index1 >= 0)
                        array1[array1.Length - 1 - index1] = new Color(0, 0, 0, 0);
                    index1++;
                }
                for (int j = 0; j < _width; j++)
                {
                    var pos2 = _topTriangleP1 + new Vector3(j, i);
                    //上半部分竹子从左下角开始网上遍历,在三角形内得切掉,要判定的是左下角部分
                    //先判断坐标是否在纹理范围内
                    if ((common1 > 0 || common2 > 0) && !MathfUtil.IsVector2PointInMatrix(new Vector3(-texture2.width / 2, -texture2.height / 2), new Vector3(texture2.width / 2, -texture2.height / 2), new Vector3(texture2.width / 2, texture2.height / 2), new Vector3(-texture2.width / 2, texture2.height / 2), pos2)) continue;
                    //进入纹理范围后从索引0开始剪切
                    //上半部分在裁剪区域内的裁剪掉
                    if (MathfUtil.Vector2IsInTriangle(_topTriangleP1, _topTriangleP2, _topTriangleP3, pos2)
                        && array2.Length > index2 && index2 >= 0)
                        array2[index2] = new Color(0, 0, 0, 0);
                    //此处索引从0开始增加
                    index2++;
                }
            }
            texture1.SetPixels(array1);
            texture2.SetPixels(array2);
            texture1.Apply();
            texture2.Apply();
            var rigi = obj2.GetComponent<Rigidbody2D>();
            obj2.transform.DOMove(obj2.transform.position + new Vector3(0.85f, -0.8f, 0), _dropTime).SetEase(Ease.InSine).OnComplete(() =>
            {
                Time.timeScale = _timeScale;
                rigi.gravityScale = _gravity;
                tweener = obj2.transform.DORotate(new Vector3(0, 0, _dropAngle), _dropTime).SetEase(_dropEase).OnComplete(() =>
                 {
                     if (obj2 != null)
                         obj2.SetActive(false);
                     if (callback != null)
                     {
                         callback.Invoke();
                     }
                 });
            });
            gameObject.SetActive(false);
        }
        public void SetSize()
        {
            _texture2D = _spriteRenderer.sprite.texture;
            _width = _texture2D.width;
            _heigth = _texture2D.height;
            _pixels = _texture2D.GetPixels();
            _lossyScale = transform.lossyScale;
        }

        Tweener tweener;
        private void OnEnable()
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
        }
        IEnumerator DrawLine(Vector2 bottomTriangleP1, Vector2 bottomTriangleP2, Vector2 bottomTriangleP3, Color color)
        {
            while (true)
            {
                yield return null;
                Debug.DrawLine(bottomTriangleP1 / 100, bottomTriangleP2 / 100, color);
                Debug.DrawLine(bottomTriangleP1 / 100, bottomTriangleP3 / 100, color);
                Debug.DrawLine(bottomTriangleP2 / 100, bottomTriangleP3 / 100, color);
            }
        }

        IEnumerator TestDrawLine()
        {
            while (true)
            {
                yield return null;
                var point1 = new Vector2(0, 0);
                var point2 = new Vector2(3, 0);
                var point3 = new Vector2(0, 4);
                Debug.DrawLine(point1, point2, Color.red);
                Debug.DrawLine(point1, point3, Color.red);
                Debug.DrawLine(point2, point3, Color.red);
            }
        }
    }
}

