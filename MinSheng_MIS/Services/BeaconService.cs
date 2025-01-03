using MinSheng_MIS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using MinSheng_MIS.Models.ViewModels;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class BeaconService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public BeaconService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 獲取樓層的Beacon模型資料
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fsn"></param>
        /// <returns></returns>
        public IEnumerable<T> GetBeaconsBimInfo<T>(string fsn)
            where T : class, new()
        {
            return _db.BeaconDevice.Where(x => x.FSN == fsn)
                .AsEnumerable()
                .Select(x => x.ToDto<BeaconDevice, T>());
        }
        #endregion

        #region 獲取Beacon Device資訊
        /// <summary>
        /// 獲取Beacon Device資訊
        /// </summary>
        /// <typeparam name="T">接取Beacon Position資訊的資料結構</typeparam>
        /// <param name="beacons">Beacon列表(必須包含Minor)</param>
        /// <returns>回傳<see cref="T"/>結構的列表</returns>
        /// <remarks>
        /// T的資料結構可包含IBeacon的屬性及[dbo].[BeaconDevice]中的屬性
        /// </remarks>
        public async Task<IEnumerable<T>> GetBeaconPositionAsync<T>(IEnumerable<IBeacon> beacons)
            where T : class, new()
        {
            var result = new List<T>();

            // 預先查詢所有 BeaconDevice，避免在循環中多次查詢資料庫
            var beaconDataLookup = await _db.BeaconDevice.ToDictionaryAsync(x => x.Minor);

            return beacons.Select(x =>
            {
                T item = x.ToDto<IBeacon, T>();
                if (beaconDataLookup.TryGetValue(x.Minor, out var beaconData))
                    item = beaconData.ToDto<BeaconDevice, T>();
                return item;
            });
        }
        #endregion

        #region 新增BeaconData紀錄
        public void AddBeaconDatas(IEnumerable<IBeacon> beacons)
        {
            // 使用者名稱
            var userName = HttpContext.Current.User.Identity.Name;

            // 新增使用者定位資訊
            
        }
        #endregion

        #region 取得有效Beacon List
        /// <summary>
        /// 將Beacon List進行篩選，獲取有效Beacon
        /// </summary>
        /// <typeparam name="T">Beacon List的Model</typeparam>
        /// <param name="beacons">Beacon List</param>
        /// <param name="threeDimensional">是否為三維空間(Z為樓層)</param>
        /// <returns></returns>
        /// <exception cref="MyCusResException">無法定位所在樓層</exception>
        public IEnumerable<T> GetValidateBeacon<T>(IEnumerable<T> beacons, bool threeDimensional = false)
            where T : IBeaconPosition
        {
            // 1. 篩選 X 和 Y 不為空值
            var validBeacons = beacons.Where(x => x.X.HasValue && x.Y.HasValue);
            // 2. 若為三維空間，篩選Z軸。(進行樓層過半數篩選)
            if (threeDimensional && validBeacons?.Any() == true)
            {
                // 找出數量過半數的樓層
                var floorGroup = validBeacons.GroupBy(x => x.FSN)
                    .Select(g => new
                    {
                        Floor = g.Key,
                        Count = g.Count(),
                        Beacons = g.ToList()
                    });
                int maxCount = floorGroup.Max(x => x.Count); // 最大數量
                var majorityFloors = floorGroup.Where(x => x.Count == maxCount);
                if (majorityFloors.Count() > 1)
                {
                    var floors = string.Join(", ", majorityFloors.Select(f => f.Floor));
                    throw new MyCusResException($"無法定位所在樓層！(Beacon所在樓層未過半數：{floors})");
                }

                validBeacons = majorityFloors.First().Beacons;
            }

            return validBeacons;
        }
        #endregion

        #region 將Beacon組合成子集合
        /// <summary>
        /// 將Beacon組合成子集合
        /// </summary>
        /// <typeparam name="T">List的資料結構</typeparam>
        /// <param name="items">Beacon List</param>
        /// <param name="size">子集合元素數量</param>
        /// <returns>將item以size進行排列組合</returns>
        /// <remarks>
        /// item = [a,b,c]; size = 2;
        /// result = {[a,b], [b,c], [a,c]}
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> GenerateSubsets<T>(IEnumerable<T> items, int size)
        {
            // 如果 size 是 0，返回空集合
            if (size == 0) return new[] { Enumerable.Empty<T>() };

            // 如果元素不足，直接返回空集合
            if (items.Count() < size) return Enumerable.Empty<IEnumerable<T>>();

            // 遞歸生成組合：選擇或跳過當前元素
            return items.SelectMany((item, index) =>
                GenerateSubsets(items.Skip(index + 1), size - 1)
                    .Select(subset => new[] { item }.Concat(subset)));
        }
        #endregion

        #region 計算子集合的中心座標
        public static (double, double)? CalculateDevicePosition(List<BeaconPosition> beacons)
        {
            if (beacons?.Count < 3)
                throw new MyCusResException("未偵測到至少三個有效Beacon，無法進行定位計算！");

            // 初始猜測位置：以信標的平均座標作為起點
            var initialGuess = Vector<double>.Build.Dense(new[]
            {
                beacons.Average(b => b.X.Value),
                beacons.Average(b => b.Y.Value)
            });

            // 目標函數：根據每個信標的位置和距離計算誤差
            //Func<Vector<double>, double> objective = position =>
            //{
            //    double sumSquaredErrors = 0;
            //    foreach (var beacon in beacons)
            //    {
            //        double expectedDistance = Math.Sqrt(
            //            Math.Pow(position[0] - beacon.X.Value, 2) +
            //            Math.Pow(position[1] - beacon.Y.Value, 2)
            //        );
            //        sumSquaredErrors += Math.Pow(expectedDistance - beacon.Distance, 2);
            //    }
            //    return sumSquaredErrors;
            //};
            Vector<double> residuals(Vector<double> parameters, Vector<double> observed)
            {
                double x = parameters[0];
                double y = parameters[1];

                var residualsVector = new double[beacons.Count];
                for (int i = 0; i < beacons.Count; i++)
                {
                    var beacon = beacons[i];
                    double bx = (double)beacon.X;
                    double by = (double)beacon.Y;
                    double d = beacon.Distance;

                    // 殘差為 (計算距離 - 實際距離)
                    residualsVector[i] = Math.Sqrt(Math.Pow(x - bx, 2) + Math.Pow(y - by, 2)) - d;
                }

                return Vector<double>.Build.Dense(residualsVector);
            }


            // Jacobian 函數：每個信標對設備位置的偏導數
            //Func<Vector<double>, Vector<double>> gradient = position =>
            //{
            //    var grad = Vector<double>.Build.Dense(2);
            //    foreach (var beacon in beacons)
            //    {
            //        double dx = position[0] - beacon.X.Value;
            //        double dy = position[1] - beacon.Y.Value;
            //        double distance = Math.Sqrt(dx * dx + dy * dy);

            //        if (distance > 0)
            //        {
            //            double factor = 2 * (distance - beacon.Distance) / distance;
            //            grad[0] += factor * dx;
            //            grad[1] += factor * dy;
            //        }
            //    }
            //    return grad;
            //};
            Matrix<double> jacobian(Vector<double> parameters, Vector<double> observed)
            {
                double x = parameters[0];
                double y = parameters[1];

                var jacobianMatrix = Matrix<double>.Build.Dense(beacons.Count, 2);
                for (int i = 0; i < beacons.Count; i++)
                {
                    var beacon = beacons[i];
                    double bx = (double)beacon.X;
                    double by = (double)beacon.Y;
                    double distance = Math.Sqrt(Math.Pow(x - bx, 2) + Math.Pow(y - by, 2));

                    // 避免除以 0
                    if (distance == 0)
                    {
                        jacobianMatrix[i, 0] = 0;
                        jacobianMatrix[i, 1] = 0;
                    }
                    else
                    {
                        jacobianMatrix[i, 0] = (x - bx) / distance; // 對 x 的偏導數
                        jacobianMatrix[i, 1] = (y - by) / distance; // 對 y 的偏導數
                    }
                }

                return jacobianMatrix;
            }

            try
            {
                // 設置優化器
                //var optimizer = new NewtonMinimizer(1e-6, 1000);
                var optimizer = new LevenbergMarquardtMinimizer
                {
                    GradientTolerance = 1e-6,   // 梯度容忍度
                    StepTolerance = 1e-6,       // 步長容忍度
                    FunctionTolerance = 1e-6,   // 函數值容忍度
                    MaximumIterations = 1000    // 最大迭代次數
                };

                // 執行優化
                //var objective_function = ObjectiveFunction.Gradient(objective, gradient);
                var observedX = Vector<double>.Build.DenseOfArray(beacons.Select(b => b.X.Value).ToArray());
                var observedY = Vector<double>.Build.DenseOfArray(beacons.Select(b => b.Y.Value).ToArray());
                var objectiveModel = ObjectiveFunction.NonlinearModel(residuals, jacobian, observedX, observedY);
                var result = optimizer.FindMinimum(objectiveModel, initialGuess);
                //var result = optimizer.FindMinimum(objective_function, initialGuess);

                // 檢查結果是否收斂
                if (result.ReasonForExit == ExitCondition.Converged ||          // 完全收斂
                    result.ReasonForExit == ExitCondition.RelativeGradient ||   // 相對梯度收斂
                    result.ReasonForExit == ExitCondition.RelativePoints ||     // 相對點收斂
                    result.ReasonForExit == ExitCondition.AbsoluteGradient)     // 絕對梯度收斂
                {
                    return (result.MinimizingPoint[0], result.MinimizingPoint[1]);
                }

                return null;
            }
            catch (OptimizationException ex)
            {
                // 優化失敗時記錄錯誤
                Console.WriteLine($"Optimization failed: {ex.Message}");
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
    }
}