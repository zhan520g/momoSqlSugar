    <template>
        <div class="container">
          <tags />
          <div class="contents">
            <transition name="fade-transform" mode="out-in">
                <router-view></router-view>
            </transition>
          </div>
        </div>
    </template>

<script>
    import Tags from './Tags.vue'
    export default {
        components:{
          Tags
        }
    }

</script>
<style lang="scss" scoped>
.container {
  height: calc(100vh - 50px);
  position: relative;
  overflow: hidden;
  // background: #f0f0f0;
  width: 100%;
  overflow: hidden;
  .contents{
    // height: calc(100vh - 78px);
    -webkit-box-sizing: border-box;
    box-sizing: border-box;
  }
}
</style>