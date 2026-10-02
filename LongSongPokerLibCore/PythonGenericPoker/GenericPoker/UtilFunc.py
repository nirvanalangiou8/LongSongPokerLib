from typing import List, TypeVar, Optional, Callable, Set, Any, Tuple

T = TypeVar('T')


class UtilFunc:
    @staticmethod
    def get_permutation(input_list: List[T], select_count: int) -> List[List[T]]:
        result_list: List[List[T]] = []
        UtilFunc._recursive_permute(input_list, select_count, [], result_list)
        return result_list

    @staticmethod
    def _recursive_permute(input_list: List[T], select_count: int, current_list: List[T], result_list: List[List[T]]) -> None:
        if len(current_list) + len(input_list) < select_count:
            return

        if len(current_list) >= select_count:
            result_list.append(list(current_list))
            return

        for i in range(len(input_list)):
            current_list.append(input_list[i])
            remaining_list = input_list[i + 1:]
            UtilFunc._recursive_permute(remaining_list, select_count, current_list, result_list)
            current_list.pop()

    @staticmethod
    def get_exclude_list(whole_list: List[T], exclude_list: List[T], key_func: Optional[Callable[[T], Any]] = None) -> List[T]:
        # Emulates C# HashSet<T>(wholeList, comparer) with ExceptWith
        exclude_items = list(exclude_list)
        result: List[T] = []
        for item in whole_list:
            found_idx = -1
            for idx, ex in enumerate(exclude_items):
                if item == ex:
                    found_idx = idx
                    break
            if found_idx >= 0:
                pass
            else:
                result.append(item)
        return result

    @staticmethod
    def get_permutation_allowed_duplicated(input_list: List[T], select_count: int) -> List[Tuple[List[T], List[T]]]:
        result_list: List[Tuple[List[T], List[T]]] = []
        UtilFunc._recursive_permute_allowed_duplicated(input_list, 0, select_count, [], list(input_list), result_list)
        return result_list

    @staticmethod
    def _recursive_permute_allowed_duplicated(whole_list: List[T], start_index: int, select_count: int,
                                              current_selected: List[T], current_remaining: List[T],
                                              result_list: List[Tuple[List[T], List[T]]]) -> None:
        if len(current_selected) == select_count:
            result_list.append((list(current_selected), list(current_remaining)))
            return

        for i in range(start_index, len(whole_list)):
            item = whole_list[i]
            current_selected.append(item)

            next_remaining = list(current_remaining)
            found_idx = -1
            for idx, rem_item in enumerate(next_remaining):
                if rem_item == item:
                    found_idx = idx
                    break
            if found_idx >= 0:
                next_remaining.pop(found_idx)

            UtilFunc._recursive_permute_allowed_duplicated(whole_list, i + 1, select_count, current_selected, next_remaining, result_list)
            current_selected.pop()
