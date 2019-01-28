using UnityEngine;
using System.Collections.Generic;

public partial class Tower : MonoBehaviour
{
	#region Backing up + restoring blocks

	/// <summary> Info needed to recreate blocks when adding a column </summary>
	class SavedBlockInfo
	{
		public SavedBlockInfo(int _col, int _row, int _blockID, float _fallingOffset)
		{
			col = _col;
			row = _row;
			blockID = _blockID;
			fallingOffset = _fallingOffset;
		}

		public int col, row;
		public int blockID;
		public float fallingOffset;
	};

	/// <summary> Backs up the blocks into an info list </summary>
	/// <returns> List of BlockInfo classes describing the blocks </returns>
	List<SavedBlockInfo> BackupBlocks()
	{
		List<SavedBlockInfo> blockInfoList = new List<SavedBlockInfo>();
		blockInfoList.Clear();
		if (blocks != null)
		{
			for (int i = 0; i < blocks.Length; ++i)
			{
				if (blocks[i] != null)
				{
					Block block = blocks[i];
					blockInfoList.Add(new SavedBlockInfo(block.col, block.row, block.blockID, block.fallingOffset));
				}
			}
		}

		return blockInfoList;
	}

	/// <summary> Restores the blocks from an info list </summary>
	/// <param name='blockInfoList'> List of BlockInfo classes to restore from </param>
	void RestoreBlocks(List<SavedBlockInfo> blockInfoList)
	{
		foreach (SavedBlockInfo info in blockInfoList)
		{
			blocks[BlockIdx(info.col, info.row)] = GetNewBlock(info.blockID, info.col, info.row);
			GetBlock(info.col, info.row).fallingOffset = info.fallingOffset;
		}
	}

	#endregion // Backing up + restoring blocks

	#region Controls

	public class ControlData
	{
		public enum HorizontalDirs { None, Left, Right };
		public enum VerticalDirs { None, Up, Down };

		public HorizontalDirs horizontalDir = HorizontalDirs.None;
		public VerticalDirs verticalDir = VerticalDirs.None;
		public bool switchBlocks = false;
	}

	#endregion // Controls
}
