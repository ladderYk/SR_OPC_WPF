<script setup>
import { ref, computed, onMounted } from "vue";
import DeviceType from "./DeviceType/index.vue";
import Device from "./Device/index.vue";
import {
  Fold, Expand
} from '@element-plus/icons-vue'
// import Home from "./pages/Index.vue";
// import About from './About.vue'
// import NotFound from './NotFound.vue'
const textarea = ref([]);
const isCollapse = ref(false)

const routes = {
  "/": DeviceType,
  "/Device": Device,
};
const currentPath = ref(window.location.hash || "#/");
window.addEventListener("hashchange", () => {
  currentPath.value = window.location.hash;
});
const currentView = computed(() => {
  return routes[currentPath.value.slice(1) || "/"];
});
const handleSelect = (key, keyPath) => {
  window.location.hash = key;
}
</script>

<template>
  <el-container>
    <el-header class="flex">
      <div class="logo">
        SR_OPC系统
      </div>
    </el-header>
    <el-container>
      <el-aside class="sf-aside">
        <el-menu class="menu" :default-active="currentPath" @select="handleSelect" style="height: calc(100vh - 100px);"
          active-text-color="#1890ff" background-color="#001529" text-color="#fff" :collapse="isCollapse">
          <el-menu-item index="#/">
            <el-icon>
              <Operation />
            </el-icon>
            <span>设备协议</span>
          </el-menu-item>
          <el-menu-item index="#/Device">
            <el-icon>
              <Monitor />
            </el-icon>
            <span>设备列表</span>
          </el-menu-item>
        </el-menu>
        <div>

          <el-button  color="#606266" :icon="isCollapse ? Expand : Fold" @click="isCollapse = !isCollapse" class="coll-icon" />
        </div>
      </el-aside>
      <el-main>
        <!-- <el-col :span="18"> -->
        <component :is="currentView" />
        <!-- </el-col> -->
        <!-- <el-row style="height: 100%;">
         
          <el-col :span="6" style="height: 100%;">
            <el-card header="消息列表" style="height: 100%;box-sizing: border-box;">
              <el-scrollbar>
                <div v-for="v in textarea">
                  <span style="font-size: 12px;">{{ v }}</span>
                </div>
              </el-scrollbar>
            </el-card>
          </el-col>
        </el-row> -->
      </el-main>
    </el-container>
  </el-container>
</template>
<style scoped>
main {
  height: calc(100vh - 60px);
}

.el-main {
  padding: 0;
}

/* .agv_main {
    height: calc(100% - 20px);
    background-color: #fff;
  } */

header {
  background-color: #001529;
  color: #fff;
  padding: 0;
}

.flex {
  display: flex;
}

.logo {
  line-height: 60px;
  font-size: 18px;
  text-align: center;
  width: 200px;
}

.coll-icon {
  justify-self: end;
  display: flex;
  margin-right: 10px;
  font-size: 18px;
}

.menu:not(.el-menu--collapse) {
  width: 200px;
}

.sf-aside {
  background-color: #001529;
  flex-basis: content;
}
</style>