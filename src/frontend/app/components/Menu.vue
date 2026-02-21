<template>
  <div ref="target">
    <slot :close="close"></slot>
  </div>
</template>

<script setup lang="ts">
const emit = defineEmits(["close"]);

function close() {
  emit("close");
}
</script>