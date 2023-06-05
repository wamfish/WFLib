//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

global using Godot;
global using WFLib;
global using static WFLib.Global;
namespace WFLib;
public enum Status { Ok, NegativeId, DuplicateId, DuplicateKey, IdNotFound, DataChangedBeforeUpdate, DataChangedBeforeDelete, DeleteInactive, ReadInactive, KeyNotFound, CanNotDeleteID0, NoChange }