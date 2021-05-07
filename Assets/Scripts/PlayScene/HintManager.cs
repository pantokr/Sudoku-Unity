using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public SudokuController sudokuController;
    public HintDialogManager hintDialogManager;
    public AutoMemoManager autoMemoManager;
    public CellManager cellManager;
    public MemoManager memoManager;

    private List<GameObject> hintCell = new List<GameObject>();
    private List<List<GameObject>> hintCellList = new List<List<GameObject>>();
    private bool breaker = false;
    private GameObject[,] objects;
    private GameObject[,,,] memoObjects;

    private void Start()
    {
        objects = cellManager.GetObjects();
        memoObjects = memoManager.GetWholeMemoObjects();
    }

    public void RunHint()
    {
        breaker = false;
        if (Settings.PlayMode == 0)
        {
            RunBasicHint();
        }
        else
        {
            if (!sudokuController.IsNormalSudoku())
            {
                string[] _str = { "오류가 있습니다." };
                hintDialogManager.StartDialog(_str);
            }
        }
        if (breaker)
        {
            return;
        }

        FindFullHouse();
        if (breaker)
        {
            return;
        }

        FindHiddenSingle();
        if (breaker)
        {
            return;
        }

        FindCrossPointing();
        if (breaker)
        {
            return;
        }

        FindNakedPair();
        if (breaker)
        {
            return;
        }

        if (Settings.PlayMode == 0)
        {
            FindFromFullSudoku();
            if (breaker)
            {
                return;
            }
        }
        else
        {
            string[] str = { "더 이상 힌트가 없습니다.\n" +
                "(메모가 작동되고 있다면 다른 힌트를 얻을 수 있습니다.)"};
            hintDialogManager.StartDialog(str);
            return;
        }
    }

    public void RunBasicHint()
    {
        bool flag;
        List<Vector2Int> points;

        // 오류 검사
        (flag, points) = sudokuController.CompareWithFullSudoku();
        if (flag)
        {
            breaker = true;

            //대사
            string[] str = { "오류가 있습니다.", "스도쿠를 수정합니다." };
            hintDialogManager.StartDialog(str);

            //처방
            foreach (var point in points)
            {
                cellManager.DeleteCell(point.y, point.x);
            }

        }
        if (breaker)
        {
            return;
        }

        //메모 충분 검사
        (flag, points) = sudokuController.CompareMemoWithFullSudoku();
        if (flag)
        {
            breaker = true;

            //대사
            string[] str = { "메모가 불충분합니다.", "메모를 수정합니다." };
            hintDialogManager.StartDialog(str);

            //처방
            autoMemoManager.RunAutoMemo();
        }
        if (breaker)
        {
            return;
        }
    }

    private void FindFullHouse() //한 셀만 부족하면
    {
        //row 검사
        for (int _y = 0; _y < 9; _y++)
        {
            var ev = sudokuController.GetEmptyValueInRow(_y);
            if (ev.Count == 1)
            {
                int _x = sudokuController.GetEmptyCellsInRow(_y)[0];
                int val = ev[0];
                breaker = true;

                //대사
                string[] str = { "풀 하우스", $"가로 행에 한 값 {ev[0]}만 비어 있습니다." };
                hintCell.Clear();
                hintCell.Add(null);
                hintCell.Add(objects[_y, _x]);

                //처방
                List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val));

                hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                return;
            }
        }

        //col 검사
        for (int _x = 0; _x < 9; _x++)
        {
            var ev = sudokuController.GetEmptyValueInCol(_x);
            if (ev.Count == 1)
            {
                int _y = sudokuController.GetEmptyCellsInCol(_x)[0];
                int val = ev[0];
                breaker = true;

                //대사
                string[] str = { "풀 하우스", $"세로 열에 한 값 {ev[0]}만 비어 있습니다." };
                hintCell.Clear();
                hintCell.Add(null);
                hintCell.Add(objects[_y, _x]);

                //처방
                List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val));

                hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                return;
            }
        }

        //서브그리드
        for (int _y = 0; _y < 3; _y++)
        {
            for (int _x = 0; _x < 3; _x++)
            {
                var ev = sudokuController.GetEmptyValueInSG(_y, _x);
                if (ev.Count == 1)
                {
                    Vector2Int cell = sudokuController.GetEmptyCellsInSG(_y, _x)[0];
                    int val = ev[0];
                    breaker = true;

                    //대사
                    string[] str = { "풀 하우스", $"3X3 서브그리드에 한 값 {ev[0]}만 비어 있습니다." };
                    hintCell.Clear();
                    hintCell.Add(null);
                    hintCell.Add(objects[cell.y, cell.x]);

                    //처방
                    List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                    toFill.Add(new Tuple<Vector2Int, int>(cell, val));

                    hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                    return;
                }
            }
        }
    }

<<<<<<< HEAD
    public bool FindHiddenSingle(bool isAutoSingle = false)
=======
    public bool FindHiddenSingle(bool isAutoSingle = false) //히든 싱글
>>>>>>> parent of 4e49a48 (update intersect claiming)
    {
        //row 검사
        for (int val = 0; val < 9; val++)
        {
            for (int _y = 0; _y < 9; _y++)
            {
                var ev = sudokuController.GetMemoRow(val, _y);
                if (ev.Sum() == 1)
                {
                    breaker = true;
                    int _x = Array.IndexOf(ev, 1);

                    if (isAutoSingle)
                    {
                        cellManager.FillCell(_y, _x, val + 1);
                        return true;
                    }
                    else // 일반
                    {
                        //대사
                        string[] str = { "히든 싱글", $"가로 행에서 이 셀에 들어갈 수 있는 값은 {val + 1} 하나입니다." };
                        hintCell.Clear();
                        hintCell.Add(null);
                        hintCell.Add(objects[_y, _x]);

                        //처방
                        List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                        toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val + 1));

                        hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                        return true;

                    }
                }
            }
        }

        //col 검사
        for (int val = 0; val < 9; val++)
        {
            for (int _x = 0; _x < 9; _x++)
            {
                var ev = sudokuController.GetMemoCol(val, _x);
                if (ev.Sum() == 1)
                {
                    breaker = true;
                    int _y = Array.IndexOf(ev, 1);

                    if (isAutoSingle)
                    {
                        cellManager.FillCell(_y, _x, val + 1);
                        return true;
                    }
                    else // 일반
                    {
                        //대사
                        string[] str = { "히든 싱글", $"세로 열에서 이 셀에 들어갈 수 있는 값은 {val + 1} 하나입니다." };
                        hintCell.Clear();
                        hintCell.Add(null);
                        hintCell.Add(objects[_y, _x]);

                        //처방
                        List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                        toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val + 1));

                        hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                        return true;

                    }
                }
            }
        }

        //SG 검사
        for (int val = 0; val < 9; val++)
        {
            for (int _y = 0; _y < 3; _y++)
            {
                for (int _x = 0; _x < 3; _x++)
                {
                    var ev = sudokuController.GetMemoSG(val, _y, _x);
                    if (ev.Sum() == 1)
                    {
                        breaker = true;

                        if (isAutoSingle)
                        {
                            cellManager.FillCell(_y, _x, val + 1);
                            return true;
                        }
                        else // 일반
                        {
                            //대사
                            string[] str = { "히든 싱글", $"3X3 서브그리드에서 이 셀에 들어갈 수 있는 값은 {val + 1} 하나입니다." };
                            hintCell.Clear();
                            hintCell.Add(null);
                            hintCell.Add(objects[_y, _x]);

                            //처방
                            List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                            toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val + 1));

                            hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                            return true;

                        }
                    }
                }
            }
        }

        return false;
    } //히든 싱글

    public bool FindNakedSingle(bool isAutoSingle = false)
    {
        for (int _y = 0; _y < 9; _y++)
        {
            for (int _x = 0; _x < 9; _x++)
            {
                if (sudokuController.IsEmptyCell(_y, _x))
                {
                    List<int> vals = new List<int>();
                    for (int val = 0; val < 9; val++)
                    {
                        if (sudokuController.IsNewValueAvailableRow(_y, _x, val + 1) == true &&
                            sudokuController.IsNewValueAvailableCol(_y, _x, val + 1) == true &&
                            sudokuController.IsNewValueAvailableSG(_y, _x, val + 1) == true)
                        {
                            vals.Add(val);
                        }
                        if (vals.Count == 2)
                        {
                            break;
                        }
                    }
                    if (vals.Count == 1)
                    {
                        breaker = true;
                        int val = vals[0];

                        if (isAutoSingle)
                        {
                            cellManager.FillCell(_y, _x, val + 1);
                            return true;
                        }
                        else // 일반
                        {
                            //대사
                            string[] str = { "네이키드 싱글", $"이 셀에 {val + 1} 말고는 들어갈 수 있는 값이 없습니다." };
                            hintCell.Clear();
                            hintCell.Add(null);
                            hintCell.Add(objects[_y, _x]);

                            //처방
                            List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                            toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val + 1));

                            hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                            return true;

                        }
                    }
                }
            }
        }
        return false;
    } //다른 영역과의 교차점

    public void FindCrossPointing()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var evs = sudokuController.GetEmptyValueInSG(y, x);
                foreach (var ev in evs)
                {
                    var (rows, cols) = sudokuController.GetLinesDisabledBySG(y, x, ev - 1);
                    //rows
                    if (rows.Count == 1)
                    {
                        int r = rows[0];

                        //교차점
                        List<int> crosses = new List<int>();
                        List<GameObject> hc = new List<GameObject>();
                        List<GameObject> dc = new List<GameObject>();
                        for (int _x = 0; _x < 9; _x++)
                        {
                            if (_x >= x * 3 && _x < x * 3 + 3) //교차점 영역이 아닐 시
                            {
                                if (sudokuController.IsInMemoCell(r, _x, ev))
                                {
                                    hc.Add(memoObjects[r, _x, (ev - 1) / 3, (ev - 1) % 3]);
                                }
                                continue;
                            }

                            if (sudokuController.IsInMemoCell(r, _x, ev) == true) //교차점 영역일 시 
                            {
                                crosses.Add(_x);
                                dc.Add(memoObjects[r, _x, (ev - 1) / 3, (ev - 1) % 3]);
                            }
                        }

                        if (crosses.Count != 0)
                        {
                            breaker = true;

                            //대사
                            string[] str = {
                                "교차점(Pointing)", $"{r + 1}행은  강조된 셀 이외에는 {ev} 값이 들어갈 수 없습니다.",
                            "그래서 다음 메모셀들을 삭제합니다."};

                            hintCellList.Clear();
                            hintCellList.Add(null);
                            hintCellList.Add(hc);
                            hintCellList.Add(dc);

                            //처방
                            List<GameObject> toDelete = new List<GameObject>();
                            hintDialogManager.StartDialogAndDeleteMemo(str, hintCellList, dc);
                            return;
                        }
                    }

                    //cols
                    if (cols.Count == 1)
                    {
                        int c = cols[0];

                        //교차점
                        List<int> crosses = new List<int>();
                        List<GameObject> hc = new List<GameObject>();
                        List<GameObject> dc = new List<GameObject>();
                        for (int _y = 0; _y < 9; _y++)
                        {
                            if (_y >= y * 3 && _y < y * 3 + 3) //교차점 영역이 아닐 시
                            {
                                if (sudokuController.IsInMemoCell(_y, c, ev))
                                {
                                    hc.Add(memoObjects[_y, c, (ev - 1) / 3, (ev - 1) % 3]);
                                }
                                continue;
                            }

                            if (sudokuController.IsInMemoCell(_y, c, ev) == true) //교차점 영역일 시 
                            {
                                crosses.Add(_y);
                                dc.Add(memoObjects[_y, c, (ev - 1) / 3, (ev - 1) % 3]);
                            }
                        }

                        if (crosses.Count != 0)
                        {
                            breaker = true;

                            //대사
                            string[] str = {
                                "교차점(Pointing)", $"{c + 1}열은  강조된 셀 이외에는 {ev} 값이 들어갈 수 없습니다.",
                            "그래서 다음 셀들을 삭제합니다."};

                            hintCellList.Clear();
                            hintCellList.Add(null);
                            hintCellList.Add(hc);
                            hintCellList.Add(dc);

                            //처방
                            List<GameObject> toDelete = new List<GameObject>();
                            hintDialogManager.StartDialogAndDeleteMemo(str, hintCellList, dc);
                            return;
                        }
                    }
                }
            }
        }
<<<<<<< HEAD
=======
    }// 인터섹션

    public bool FindNakedSingle(bool isAutoSingle = false)
    {
        for (int _y = 0; _y < 9; _y++)
        {
            for (int _x = 0; _x < 9; _x++)
            {
                if (sudokuController.IsEmptyCell(_y, _x))
                {
                    var mv = sudokuController.GetActiveMemoValue(_y, _x);

                    if (mv.Count == 1)
                    {
                        breaker = true;
                        int val = mv[0];

                        if (isAutoSingle)
                        {
                            cellManager.FillCell(_y, _x, val);
                            return true;
                        }
                        else // 일반
                        {
                            //대사
                            string[] str = { "네이키드 싱글", $"이 셀에 {val} 말고는 들어갈 수 있는 값이 없습니다." };

                            //처방
                            var hc = MakeHC(null, objects[_y, _x]);
                            var toFill = MakeTuple((_y, _x), val);

                            hintDialogManager.StartDialogAndFillCell(str, hc, toFill);
                            return true;

                        }
                    }
                }
            }
        }
        return false;
>>>>>>> parent of 4e49a48 (update intersect claiming)
    }

    public void FindNakedPair()
    {
        //row
        for (int y = 0; y < 9; y++)
        {
            var ec_xs = sudokuController.GetEmptyCellsInRow(y); //비어 있는 x좌표
            var mvList = sudokuController.GetMemoValuesInRow(y); //메모 안에 들어있는 값들의 모음
            for (int _x1 = 0; _x1 < ec_xs.Count - 1; _x1++)
            {
                for (int _x2 = _x1 + 1; _x2 < ec_xs.Count; _x2++)
                {
                    if (mvList[_x1].Count != 2 || mvList[_x2].Count != 2)
                    {
                        continue;
                    }

                    if (sudokuController.IsEqualMemoCell(new Vector2Int(y, ec_xs[_x1]), new Vector2Int(y, ec_xs[_x2])))
                    {
                        print($"{y},{ec_xs[_x1]}  {y},{ec_xs[_x2]}");
                        var mvs = mvList[_x1]; // 선택된 셀들 안에 들어있는 메모값들
                        List<GameObject> dc = new List<GameObject>();

                        foreach (var ec_x in ec_xs) // 네이키드 페어 외의 나머지 셀들 조사
                        {
                            if (ec_x == _x1 || ec_x == _x2)
                            {
                                continue;
                            }

                            foreach (var mv in mvs)
                            {
                                if (sudokuController.IsInMemoCell(y, ec_x, mv))
                                {
                                    dc.Add(memoObjects[y, ec_x, (mv - 1) / 3, (mv - 1) % 3]);
                                }
                            }
                        }

                        if (dc.Count != 0)
                        {
                            breaker = true;

                            List<GameObject> hc = new List<GameObject>();
                            hc.Add(objects[y, _x1]);
                            hc.Add(objects[y, _x2]);

<<<<<<< HEAD
=======
                            foreach (var mv in mvs)
                            {
                                hc.Add(memoObjects[y, emptyXList[_x1], (mv - 1) / 3, (mv - 1) % 3]);
                                hc.Add(memoObjects[y, emptyXList[_x2], (mv - 1) / 3, (mv - 1) % 3]);
                            }
>>>>>>> parent of 4e49a48 (update intersect claiming)

                            //대사
                            string[] str = {
                                "네이키드 페어",
                            $"{y+1} 행의 강조된 두 셀에 값 {mvs[0]}, {mvs[1]}으로 이루어진 똑같은 구성의 메모 셀이 존재합니다",
                            $"이는 값 {mvs[0]}, {mvs[1]}이 이 행의 강조된 두 셀에서만 존재해야만 한다는 것을 의미합니다.",
                            "그래서 다음 메모셀들을 삭제합니다."};

                            hintCellList.Clear();
                            hintCellList.Add(null);
                            hintCellList.Add(hc);
                            hintCellList.Add(hc);
                            hintCellList.Add(dc);

                            //처방
                            hintDialogManager.StartDialogAndDeleteMemo(str, hintCellList, dc);

                            return;
                        }
                    }
                }
            }
        }
<<<<<<< HEAD
=======

        //SG
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var emptyYXList = sudokuController.GetEmptyCellsInSG(y, x); //비어 있는 y좌표
                var mvList = sudokuController.GetMemoValuesInSG(y, x); //메모 안에 들어있는 값들의 모음
                for (int _sg1 = 0; _sg1 < emptyYXList.Count - 1; _sg1++)
                {
                    for (int _sg2 = _sg1 + 1; _sg2 < emptyYXList.Count; _sg2++)
                    {
                        if (mvList[_sg1].Count != 2 || mvList[_sg2].Count != 2)
                        {
                            continue;
                        }

                        if (sudokuController.IsEqualMemoCell(
                            (emptyYXList[_sg1].Item1, emptyYXList[_sg1].Item2),
                            (emptyYXList[_sg2].Item1, emptyYXList[_sg2].Item2))) // 네이키드 페어 발견
                        {
                            var mvs = mvList[_sg1]; // 선택된 셀들 안에 들어있는 메모값들

                            // 선택된 셀들 안에 들어있는 메모값들
                            List<GameObject> dc = new List<GameObject>();

                            foreach (var emptyYX in emptyYXList) // 네이키드 페어 외의 나머지 셀들 조사
                            {
                                if ((emptyYX.Item1 == emptyYXList[_sg1].Item1) && (emptyYX.Item2 == emptyYXList[_sg1].Item2) ||
                                    (emptyYX.Item1 == emptyYXList[_sg2].Item1) && (emptyYX.Item2 == emptyYXList[_sg2].Item2))
                                {
                                    continue;
                                }

                                foreach (var mv in mvs)
                                {
                                    if (sudokuController.IsInMemoCell(emptyYX.Item1, emptyYX.Item2, mv))
                                    {
                                        dc.Add(memoObjects[emptyYX.Item1, emptyYX.Item2, (mv - 1) / 3, (mv - 1) % 3]);
                                    }
                                }
                            }

                            if (dc.Count != 0)
                            {
                                breaker = true;

                                List<GameObject> hc = new List<GameObject>();

                                foreach (var mv in mvs)
                                {
                                    hc.Add(memoObjects[emptyYXList[_sg1].Item1, emptyYXList[_sg1].Item2, (mv - 1) / 3, (mv - 1) % 3]);
                                    hc.Add(memoObjects[emptyYXList[_sg2].Item1, emptyYXList[_sg2].Item2, (mv - 1) / 3, (mv - 1) % 3]);
                                }

                                //대사
                                string[] str = {
                                "네이키드 페어",
                            $"{y*3+x+1} 번째 서브그리드의 강조된 두 셀은 값 {mvs[0]}, {mvs[1]}으로 이루어진 똑같은 구성의 메모 셀들입니다.",
                            $"이는 값 {mvs[0]}, {mvs[1]}이 이 열의 강조된 두 셀에서만 존재해야만 한다는 것을 의미합니다.",
                            "따라서 다음 메모 셀들을 삭제합니다."};

                                //처방
                                var hcList = MakeHCList(null, hc, hc, dc);
                                hintDialogManager.StartDialogAndDeleteMemo(str, hcList, dc);

                                return;
                            }
                        }
                    }
                }
            }
        }
>>>>>>> parent of 4e49a48 (update intersect claiming)
    }

    private void FindFromFullSudoku()
    {
        for (int _y = 0; _y < 9; _y++)
        {
            for (int _x = 0; _x < 9; _x++)
            {
                if (SudokuManager.sudoku[_y, _x] == 0)
                {
                    breaker = true;
                    int val = SudokuManager.fullSudoku[_y, _x];

                    //대사
                    string[] str = { "길라잡이", $"{_y + 1}행 {_x + 1}열의 값은 {val} 입니다." };

                    hintCell.Clear();
                    hintCell.Add(null);
                    hintCell.Add(objects[_y, _x]);

                    //처방
                    List<Tuple<Vector2Int, int>> toFill = new List<Tuple<Vector2Int, int>>();
                    toFill.Add(new Tuple<Vector2Int, int>(new Vector2Int(_x, _y), val));
                    hintDialogManager.StartDialogAndFillCell(str, hintCell, toFill);
                }
            }
        }
    }
}
