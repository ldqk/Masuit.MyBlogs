<template><q-field :label="label" :outlined="outlined" :dense="dense" :autogrow="autogrow" :class="['color-tag-input', { 'color-tag-input--dense': dense }, { 'q-field--float': hasValue }]" :style="fieldStyle" @click="focusInput">
  <div class="tag-list" :style="tagListStyle" ref="tagList">
    <q-icon v-if="clearable && modelValue.length" name="clear" class="clear-icon" @mousedown.prevent @click="clearTags" />
    <span v-for="(tag, idx) in modelValue" :key="idx" class="tag" :style="{ background: tagColor(tag) }"> {{ tag }} <q-icon name="close" class="remove-icon" @click="removeTag(idx)" />
    </span>
    <input ref="input" v-model="inputValue" :style="inputStyle" @keydown="onKeydown" @blur="addTag" class="tag-input"/>
  </div>
</q-field></template>
<script setup>
import { ref, computed, watch, nextTick } from 'vue'

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  label: { type: String, default: '' },
  outlined: { type: Boolean, default: false },
  dense: { type: Boolean, default: false },
  autogrow: { type: Boolean, default: false },
  tagColors: { type: Array, default: () => ['#FFB300', '#39B54A', '#00A1E9', '#F75000', '#8C6E63', '#E67E22'] },
  clearable: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue'])

const inputValue = ref('')
const tagList = ref(null)
const input = ref(null)

const hasValue = computed(() => {
  return props.modelValue.length > 0 || inputValue.value.length > 0
})
const fieldStyle = computed(() => ({
  minHeight: props.dense ? '40px' : '56px',
  transition: 'min-height 0.2s',
  overflow: 'visible'
}))

const tagListStyle = computed(() => ({
  display: 'flex',
  flexWrap: 'wrap',
  alignItems: 'center',
  gap: props.dense ? '4px' : '8px',
  minHeight: props.dense ? '28px' : '50px',
  padding: props.dense ? '1px 4px' : '8px 12px'
}))

const inputStyle = computed(() => ({
  border: 'none',
  outline: 'none',
  fontSize: props.dense ? '12px' : '16px',
  background: 'transparent',
  minWidth: props.dense ? '30px' : '50px',
  flexGrow: 1,
  marginLeft: '2px',
  marginRight: '2px',
  height: props.dense ? '20px' : '24px',
  width: '50px'
}))

function tagColor(tag) {
  // Hash tag to pick a color
  const idx = Math.abs(
    Array.from(tag).reduce((acc, ch) => acc + ch.charCodeAt(0), 0)
  ) % props.tagColors.length
  return props.tagColors[idx]
}

function addTag() {
  const val = inputValue.value.trim()
  if (val && !props.modelValue.includes(val)) {
    emit('update:modelValue', [...props.modelValue, val])
  }
  inputValue.value = ''
}

function removeTag(idx) {
  const tags = [...props.modelValue]
  tags.splice(idx, 1)
  emit('update:modelValue', tags)
  nextTick(() => {
    input.value && input.value.focus()
  })
}

function clearTags() {
  emit('update:modelValue', [])
  inputValue.value = ''
  nextTick(() => {
    input.value && input.value.focus()
  })
}

function onKeydown(e) {
  if (e.key === 'Enter' || e.key === ',') {
    e.preventDefault()
    addTag()
  } else if (e.key === 'Backspace' && inputValue.value.length === 0) {
    if (props.modelValue.length > 0) {
      removeTag(props.modelValue.length - 1)
    }
  }
}

function focusInput() {
  input.value && input.value.focus()
}

// Auto resize field height based on tag rows
watch(
  () => props.modelValue.length,
  () => {
    nextTick(() => {
      if (!tagList.value) return
      tagList.value.parentElement.style.minHeight =
        Math.max(fieldStyle.value.minHeight.replace('px', ''), tagList.value.offsetHeight) + 'px'
    })
  }
)
</script>
<style scoped>
.color-tag-input .q-field__control {
  padding: 0 !important;
}

.tag-list {
  width: 100%;
  box-sizing: border-box;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  cursor: text;
  /* ↓↓↓ 这里设置 dense 高度和间距 ↓↓↓ */
  min-height: 24px !important;
  padding: 0px 2px !important;
}

.color-tag-input--dense .tag-list {
  min-height: 28px;
  padding: 1px 4px;
}

.tag {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  margin: 1px 0;
  border-radius: 14px;
  color: #fff;
  font-size: 13px;
  user-select: none;
  white-space: nowrap;
  transition: background 0.2s;
  height: 24px;
}

.color-tag-input--dense .tag {
  padding: 1px 6px;
  font-size: 12px;
  border-radius: 12px;
  height: 20px;
}

.tag .remove-icon {
  margin-left: 4px;
  font-size: 14px;
  cursor: pointer;
}

.tag-input {
  background: transparent;
  border: none;
  outline: none;
  min-width: 20px;
  font-size: 13px;
  margin: 1px;
  padding: 0;
  flex-grow: 1;
  height: 24px;
}

.color-tag-input--dense .tag-input {
  font-size: 12px;
  height: 20px;
  min-width: 30px;
}

.clear-icon {
  font-size: 18px;
  margin-left: 4px;
  color: #999;
  cursor: pointer;
  align-self: center;
  transition: color 0.2s;
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  background: #fff;
  border-radius: 50%;
  padding: 2px;
  z-index: 1;
}

.input-wrapper {
  position: relative;
  flex: 1;
}

.input-wrapper .tag-input {
  padding-right: 28px;
}

.clear-icon:hover {
  color: #f44336;
}
</style>
